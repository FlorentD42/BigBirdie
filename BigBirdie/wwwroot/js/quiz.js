"use strict";

// variable globales
var quizState;
var questionId = -1;
var maxTime;

const userTemplate = ({ user, style }) => `
    <div class="col">
	    <p class="p-2 ${style}">${user}</p>
    </div>`;

// objet HubConnection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/QuizHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

// reconnexion auto
connection.onclose(async () => {
    await start();
});

// update la session (joueurs, questions, scores)
connection.on("SessionUpdate", (sessionJson) => {
    var session = JSON.parse(sessionJson);
    console.log(session);

    if (session.State == "LOBBY") {
        $("#lobby").show();
        $("#question").hide();
        $("#scores").hide();

        quizState = session.State;

        $("#usersDiv").empty();
        $("#onlineSpan").text(`(${session.Users.length}/${session.MaxSize})`);

        session.Users.forEach(user => {
            var style = user.Name == session.Owner ? "owner-card" : "user-card";
            $("#usersDiv").append([{ user: user.Name, style: style }].map(userTemplate));
        });
    }
    else if (session.State == "QUESTION") {
        $("#lobby").hide();
        $("#question").show();
        $("#scores").hide();

        quizState = session.State;
        var question = session.CurrentQuestion;

        if (question.id == questionId)
            return;
        questionId = question.id;

        $("#question_text").text(question.question);
        $("#answer_0").text(question.propositions[0]);
        $("#answer_1").text(question.propositions[1]);
        $("#answer_2").text(question.propositions[2]);
        $("#answer_3").text(question.propositions[3]);

        $(".btn-check").prop("checked", false);
        $("input[type=radio][name=btnradio]").prop("disabled", false);
        $("label").removeClass("btn-success");
        $("#continueButton").prop("disabled", true);

        maxTime = session.QuestionTimer;
    }
    else if (session.State == "SCORE") {
        $("#lobby").hide();
        $("#question").hide();
        $("#scores").show();

        if (quizState == session.State)
            return; // ne pas refresh
        quizState = session.State;

        $("#scoreboard").empty();
        var pos = 1;
        var lastScore = 0;
        session.Users.forEach(user => {
            $("#scoreboard").append("<div>#" + pos + " " + user.Name + " (" + user.Score + "/" + session.NumberQuestions + ")" + "</div>");
            if (user.Score != lastScore)
                pos++;
            lastScore = user.Score;
        });
    }
});

// réception de l’id de la réponse à la question courante
connection.on("SendAnswer", (answerId) => {
    $("input[type=radio][name=btnradio]").prop("disabled", true);
    $("input:radio").removeAttr("checked");
    $("#continueButton").prop("disabled", false);

    setProgressBar(0);

    $("#answer_" + answerId).addClass("btn-success");
    quizState = "ANSWER";
});

// en cas d’erreur : alert puis redirect
connection.on("Error", (message) => {
    console.log(message);
    alert(message);
    document.location.href = "/";
});

// met à jour la progress bar via le serveur
connection.on("UpdateTimer", (timeLeft) => {
    setProgressBar((timeLeft / maxTime * 100));
});

// affichage owner/viewer
connection.on("IsOwner", () => {
    $("#startButton").show();
    $("#continueButton").show();
});

function setProgressBar(percent) {
    $("#progressbar").width(percent + "%");
}

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

    // boutons Quitter
    $("#leaveButton").click(async () => {
        if (confirm("Voulez-vous vraiment quitter ?")) {
            await connection.invoke("LeaveSession", code);
            document.location.href = "/";
        }
    });
    $("#leaveButton2").click(async () => {
        await connection.invoke("LeaveSession", code);
        document.location.href = "/";
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

    // choix d’une réponse
    $("input[type=radio][name=btnradio]").change(function () {
        var answer = $(this).val();
        if (answer != null)
            connection.invoke("SendAnswer", code, answer);
    });

    // changement dans les règles du salon
    $(".rule-select").change(function () {
        $(".rule-select option:selected").each(function () {
            console.log($(this).text());
        });
    });

}


main();