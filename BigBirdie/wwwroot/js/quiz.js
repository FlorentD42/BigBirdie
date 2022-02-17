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

// si l’hôte quitte le salon, message puis redirect
connection.on("SessionEnded", () => {
    alert("L’hôte a quitté le salon !");
    document.location.href = "/";
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
        await connection.invoke("AddToGroup", code);
    } catch (err) {
        console.error(err);
    }

    // bouton Quitter
    $("#leaveButton").click(async () => {
        if (confirm("Voulez-vous vraiment quitter ?")) {
            window.onbeforeunload = null;
            await connection.invoke("Logout", code);
            await connection.stop();
            document.location.href = "/";
        }
    });

    $("#startButton").click(async () => {
        
    });

    // confirmer avant de quitter
    window.onbeforeunload = function () {
        return "";
    };
}

main();