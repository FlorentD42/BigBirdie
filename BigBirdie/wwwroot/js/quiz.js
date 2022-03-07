"use strict";

$("#lobby").hide();
$("#question").hide();

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

    if (session.State == "LOBBY")
    {
        $("#lobby").show();
        $("#usersDiv").empty();
        $("#onlineSpan").text(`(${session.Users.length}/${session.MaxSize})`);
        session.Users.forEach(user => {
            var style = user == session.Owner ? "owner-card" : "user-card";
            $("#usersDiv").append([{ user: user, style: style }].map(userTemplate));
        });
    }
    else
    {
        var question = session.CurrentQuestion;
        $("#lobby").hide();
        $("#question").show();
        $("#question_text").text(question.question);
        $("#answer_0").text(question.propositions[0]);
        $("#answer_1").text(question.propositions[1]);
        $("#answer_2").text(question.propositions[2]);
        $("#answer_3").text(question.propositions[3]);

        $(".btn-check").prop("checked", false);
        $("input[type=radio][name=btnradio]").prop("disabled", false);
        $("label").removeClass("btn-success");
        $("#continueButton").prop("disabled", true);

        var maxDuration = 10;
        var duration = maxDuration;
        var x = setInterval(() => {
            if (duration <= 0) clearInterval(x);
            $("#progressbar").width((duration / maxDuration * 100) + "%");
            duration = duration - 0.1;
        }, 100);
    }

    
});

connection.on("SendAnswer", (answerId) => {
    $("input[type=radio][name=btnradio]").prop("disabled", true);
    $("input:radio").removeAttr("checked");
    $("#continueButton").prop("disabled", false);

    $("#answer_" + answerId).addClass("btn-success");
});

connection.on("Error", (message) => {
    console.log(message);
    alert(message);
    document.location.href = "/";
});

connection.on("UpdateTimer", (timeLeft) => {

});

// affichage owner/viewer
$("#startButton").hide();
$("#continueButton").hide();
connection.on("IsOwner", () => {
    $("#startButton").show();
    $("#continueButton").show();
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
        await connection.invoke("StartSession", code);
    });

    $("#continueButton").click(async () => {
        await connection.invoke("NextQuestion", code);
    });

    // bouton Copier
    $("#copyCode").click(async () => {
        var code = $("#sessionCode").val();
        $("#sessionCode").focus();
        $("#sessionCode").select();
        navigator.clipboard.writeText(code);
    });

    $("input[type=radio][name=btnradio]").change(function () {
        var answer = $(this).val();
        if (answer != null)
            connection.invoke("SendAnswer", code, answer);
    });

}

main();