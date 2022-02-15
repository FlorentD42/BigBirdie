using BigBirdie.Account;
using BigBirdie.Hubs;
using BigBirdie.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
	options.SignIn.RequireConfirmedAccount = false;
	options.SignIn.RequireConfirmedEmail = false;
}).AddEntityFrameworkStores<ApplicationDbContext>()
.AddRoleManager<RoleManager<ApplicationRole>>();

builder.Services.ConfigureApplicationCookie(options =>
{
	// Cookie settings
	options.LoginPath = "/";
	options.AccessDeniedPath = "/";
});

builder.Services
	.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(o => o.LoginPath = "/")
	.AddTwitch(o =>
	{
		o.ClientId = builder.Configuration["ClientId"];
		o.ClientSecret = builder.Configuration["ClientSecret"];
		o.CallbackPath = "/Twitch/Code";
		o.UserInformationEndpoint = "https://id.twitch.tv/oauth2/userinfo";
		o.SaveTokens = true;

		o.Events = new OAuthEvents
		{
			OnCreatingTicket = async context =>
			{
				var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
				request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
				request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
				response.EnsureSuccessStatusCode();

				using (var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
				{
					string userId = user.RootElement.GetString("sub");
					if (!string.IsNullOrEmpty(userId))
						context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId, ClaimValueTypes.String, context.Options.ClaimsIssuer));

					string formattedName = user.RootElement.GetString("preferred_username");
					if (!string.IsNullOrEmpty(formattedName))
						context.Identity.AddClaim(new Claim(ClaimTypes.Name, formattedName, ClaimValueTypes.String, context.Options.ClaimsIssuer));
				}
			}
		};
	});

builder.Services.AddSignalR();
builder.Services.AddServerSideBlazor();

builder.Services.AddResponseCompression(opts =>
{
	opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
		new[] { "application/octet-stream" });
});

builder.Services.AddSingleton<QuizService>();


var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapBlazorHub();
app.MapHub<QuizHub>("/QuizHub");

app.Run();
