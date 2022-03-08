"use strict";

// variable globales
var quiz_state;
var question_id = -1;
var max_time;

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

        quiz_state = session.State;

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

        quiz_state = session.State;
        var question = session.CurrentQuestion;

        if (question.id == question_id)
            return;
        question_id = question.id;


        $("#question_text").text(question.question);
        $("#answer_0").text(question.propositions[0]);
        $("#answer_1").text(question.propositions[1]);
        $("#answer_2").text(question.propositions[2]);
        $("#answer_3").text(question.propositions[3]);

        $(".btn-check").prop("checked", false);
        $("input[type=radio][name=btnradio]").prop("disabled", false);
        $("label").removeClass("btn-success");
        $("#continueButton").prop("disabled", true);

        max_time = session.QuestionTimer;
    }
    else if (session.State == "SCORE") {
        $("#lobby").hide();
        $("#question").hide();
        $("#scores").show();

        if (quiz_state == session.State)
            return; // ne pas refresh
        quiz_state = session.State;

        $("#scoreboard").empty();
        var pos = 1;
        var lastScore = 0;
        session.Users.forEach(user => {
            $("#scoreboard").append("<div>#" + pos + " " + user.Name + " (" + user.Score + "/" + session.NumberQuestions+ ")" + "</div>");
            if (user.Score != lastScore)
                pos++;
            lastScore = user.Score;
        });
    }
});

connection.on("SendAnswer", (answerId) => {
    $("input[type=radio][name=btnradio]").prop("disabled", true);
    $("input:radio").removeAttr("checked");
    $("#continueButton").prop("disabled", false);

    SetProgressBar(100);

    $("#answer_" + answerId).addClass("btn-success");
    quiz_state = "ANSWER";
});

connection.on("Error", (message) => {
    console.log(message);
    alert(message);
    document.location.href = "/";
});

connection.on("UpdateTimer", (time_left) => {
    SetProgressBar((time_left / max_time * 100));
});

// affichage owner/viewer
$("#startButton").hide();
$("#continueButton").hide();
connection.on("IsOwner", () => {
    $("#startButton").show();
    $("#continueButton").show();
});

function SetProgressBar(percent) {
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

    $("input[type=radio][name=btnradio]").change(function () {
        var answer = $(this).val();
        if (answer != null)
            connection.invoke("SendAnswer", code, answer);
    });

}


main();