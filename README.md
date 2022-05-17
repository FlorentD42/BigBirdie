# Le Big Birdie
## Mise en place
Une fois le projet cloné dans Visual Studio 2022, il faut régler deux choses avant de pouvoir le lancer en local :
- Clic droit sur le projet -> "Gérer les données secrètes de l'utilisateur" -> ajouter `"ClientId" = "<id>",` et `"ClientSecret" = "<secret>"` avec leurs valeurs obtenue via Twitch https://dev.twitch.tv/console/apps (l’URL locale de redirection est https://localhost:7076/Twitch/Code).
```
{
  "ClientId": "xxxxxxxxxxxx",
  "ClientSecret": "xxxxxxxxxxxxxxxx"
}
```
- Clic droit sur "Connected Services" et configurer un serveur local "SQL Server Express LocalDB" en ajoutant `MultipleActiveResultSets=true` dans la chaîne de connexion, par ex : `"Server=(localdb)\\mssqllocaldb;Database=aspnet-xxxx;Trusted_Connection=True;MultipleActiveResultSets=true;MultipleActiveResultSets=true"`
- Dans la "Console du Gestionnaire de package" (Outils -> Gestionnaire de package NuGet -> Console), taper la commande "Update-Database".

L’interface Admin se trouve là : https://localhost:7076/Role et n’est accessible qu’aux utilisateurs membres du rôle "Admin", par défaut chaque nouvel utilisateur est uniquement "Viewer". Pour y accéder, mettre temporairement le `[Authorize(Roles = "Admin")]` en commentaire dans Controllers/RoleController et ajoutez vous comme Admin.
