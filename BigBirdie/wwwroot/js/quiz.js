"use strict";

// objet HubConnection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/QuizHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

// reconnexion auto
connection.onclose(async () => {
    await start();
});

// à chaque nouvelle connexion/déco, mettre à jour le lobby
connection.on("SessionUpdate", (sessionJson) => {
    const userTemplate = ({ user, style }) => `
        <div class="col">
	        <div class="p-2 ${style}">${user}</div>
        </div>`;

    var session = JSON.parse(sessionJson);
    console.log(session);
    $("#usersDiv").empty();
    $("#onlineSpan").text(`(${session.Users.length}/${session.MaxSize})`);
    session.Users.forEach(user => {
        var style = user == session.Owner ? "owner-card" : "user-card";
        $("#usersDiv").append([{ user: user, style: style}].map(userTemplate));
    });
});

connection.on("Error", (message) => {
    console.log(message);
    alert(message);
    document.location.href = "/";
});

// affichage owner/viewer
$("#startButton").hide();
connection.on("IsOwner", () => {
    $("#startButton").show();
});

// connexion
async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected");
    } catch (err) {
        console.error(err);
        setTimeout(start, 5000);
    }
}

async function main() {
    // connexion
    await start();

    // ajout au groupe/salon
    try {
        await connection.invoke("JoinSession", code);
    } catch (err) {
        console.error(err);
    }

    // bouton Quitter
    $("#leaveButton").click(async () => {
        if (confirm("Voulez-vous vraiment quitter ?")) {
            await connection.invoke("LeaveSession", code);
            document.location.href = "/";
        }
    });

    // bouton Commencer
    $("#startButton").click(async () => {
        
    });

    // bouton Copier
    $("#copyCode").click(async () => {
        var code = $("#sessionCode").val();
        $("#sessionCode").focus();
        $("#sessionCode").select();
        navigator.clipboard.writeText(code);
    });

}

main();