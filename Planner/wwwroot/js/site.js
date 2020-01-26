// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function PostAvailabilities(endpoint, type, values = {}) {
    $.ajax({
        url: "/Availabilities/" + endpoint,
        type: type,
        data: values,
        success: reload,
        error: function (res) {
            if (res.responseText == "overlap") {
                alert("You already have an availability during these hours.");
            } else if (res.responseText == "bad_times") {
                alert("Incorrect times requested.");
            }
        }
    });
}

function createAvailability() {
    var values = {
        Date: $("input[name=Date]").val(),
        StartTime: $("input[name=StartTime").val(),
        EndTime: $("input[name=EndTime]").val(),
    };
    if ($('#enableSeries').is(':checked')) {
        if ($('input[name=pattern]:checked').val() == "daily") {
            if ($('input[name=dailyPattern]:checked').val() == "every") {
                values.Pattern = $("input[name=everyDays]").val();
                values.Range = $("input[name=range]").val();
                PostAvailabilities("CreateDaily", "POST", values);
            } else if ($('input[name=dailyPattern]:checked').val() == "weekday") {
                values.Range = $("input[name=range]").val();
                PostAvailabilities("createDailyWeekdays", "POST", values);
            }
        } else if ($('input[name=pattern]:checked').val() == "weekly") {
            var days = $("input[name=everyWeeksDays]:checked").map(function () {
                return $(this).val();
            }).get();
            values.Pattern = $("input[name=everyWeeks]").val();
            values.Range = $("input[name=range]").val();
            values.Days = days;

            PostAvailabilities("CreateWeekly", "POST", values);
        }
    } else {
        PostAvailabilities("Create", "POST", values);
    }

    return false;
}

function editAvailability() {
    var values = {
        StartTime: $("input[name=StartTime").val(),
        EndTime: $("input[name=EndTime]").val(),
    };
    if ($('#enableSeries').is(':checked')) {
        var series = $("input[name=Series]").val();
        PostAvailabilities("EditSeries/" + series, "POST", values);
    } else {
        var id = $("input[name=Id]").val();
        values.Date = $("input[name=Date]").val();
        PostAvailabilities("Edit/" + id, "POST", values);
    }

    return false;
}

function deleteAvailability() {
    if ($('#enableSeries').is(':checked')) {
        var series = $("input[name=Series]").val();
        PostAvailabilities("DeleteSeries/" + series, "POST");
    } else {
        var id = $("input[name=Id]").val();
        PostAvailabilities("Delete/" + id, "POST");
    }

    return false;
}

function PostShifts(endpoint, type, values = {}) {
    $.ajax({
        url: "/Shifts/" + endpoint,
        type: type,
        data: values,
        success: reload,
        error: function (res) {
            if (res.responseText == "overlap") {
                alert("You already have a shift during these hours.");
            } else if (res.responseText == "bad_times") {
                alert("Incorrect times requested.");
            }
        }
    });
}

function createShift() {
    var values = {
        Title: $("input[name=Title]").val(),
        Details: $("input[name=Details]").val(),
        Date: $("input[name=Date]").val(),
        StartTime: $("input[name=StartTime").val(),
        EndTime: $("input[name=EndTime]").val(),
    };
    if ($('#enableSeries').is(':checked')) {
        if ($('input[name=pattern]:checked').val() == "daily") {
            if ($('input[name=dailyPattern]:checked').val() == "every") {
                values.Pattern = $("input[name=everyDays]").val();
                values.Range = $("input[name=range]").val();
                PostShifts("CreateDaily", "POST", values);
            } else if ($('input[name=dailyPattern]:checked').val() == "weekday") {
                values.Range = $("input[name=range]").val();
                PostShifts("createDailyWeekdays", "POST", values);
            }
        } else if ($('input[name=pattern]:checked').val() == "weekly") {
            var days = $("input[name=everyWeeksDays]:checked").map(function () {
                return $(this).val();
            }).get();
            values.Pattern = $("input[name=everyWeeks]").val();
            values.Range = $("input[name=range]").val();
            values.Days = days;

            PostShifts("CreateWeekly", "POST", values);
        }
    } else {
        PostShifts("Create", "POST", values);
    }

    return false;
}

function reload(controller) {
    window.location.href = "/" + controller;
}

function openAssignWindow(el, date) {
    shiftId = el.parentNode.getAttribute("attr-id");

    assignWindow = document.getElementById("assignWindow");
    if (assignWindow !== null) {
        assignWindow.parentNode.removeChild(assignWindow);
    }
    assignWindow = document.createElement("div");
    assignWindow.setAttribute("id", "assignWindow");
    assignWindow.setAttribute("class", "container");
    assignWindow.setAttribute("attr-id", shiftId);

    document.getElementsByTagName("BODY")[0].appendChild(assignWindow);

    var xhr = new XMLHttpRequest();

    xhr.addEventListener("load", createAssignWindow);

    xhr.responseType = "json";

    xhr.open("GET", "/Shifts/GetAvailabilitiesForShift?ShiftId=" + encodeURIComponent(shiftId));
    xhr.send();
}

function createAssignWindow() {
    if (this.status === 200) {
        console.log(this.response);
        var assignWindow = document.getElementById("assignWindow");
        var shiftId = assignWindow.getAttribute("attr-id");

        assignWindow.innerHTML += "<h4>Available users</h4><a href=''>Back</a><br />";
        assignWindow.innerHTML += "<p style='margin-bottom: 0;'>Select an available user: </p>";

        var thead = "<thead><tr><th>User</th><th>Time</th><th>Til date</th><th></th><th></th></thead>";
        var tbody = "<tbody>";

        for (var i = 0; i < this.response.length; i++) {
            var availability = this.response[i].availability
            tbody += "<tr><td>" + availability.Username + "</td><td>" + availability.StartTime.slice(0, -3) + " - " + availability.EndTime.slice(0, -3) + "</td><td>" + this.response[i].availableTilDate + "</td><td onclick='assign(" + availability.Id + "," + shiftId + ")'>Assign</td><td onclick='assignRecurringly(" + availability.Id + "," + shiftId + "," + this.response[i].availableWeeks + ")'>Assign Recurringly</td></tr>"
        }

        tbody += "</tbody>";
        var table = "<table>" + thead + tbody + "</table>";

        assignWindow.innerHTML += table;

        var xhr = new XMLHttpRequest();

        xhr.addEventListener("load", function () {
            var assignWindow = document.getElementById("assignWindow");
            assignWindow.innerHTML += "<p style='margin-bottom: 0;'>Select any user: </p>";
            assignWindow.innerHTML += "<select id='selectUser' onchange='selectUser()'></select>";
            var select = document.getElementById("selectUser");
            select.innerHTML += "<option selected disabled>Select user</option>"
            for (var i = 0; i < this.response.length; i++) {
                select.innerHTML += "<option value='" + this.response[i].UserName + "'>" + this.response[i].UserName + "</option>"
            }
        });

        xhr.responseType = "json";

        xhr.open("GET", "/Users");
        xhr.send();

    } else {
        console.log(this.status);
    }
}

function assign(availabilityId, shiftId) {
    var xhr = new XMLHttpRequest();

    xhr.addEventListener("load", function () {
        if (this.status == 200) {
            reload("Shifts");
        } else {
            alert("Error " + this.status);
        }
    });

    xhr.responseType = "json";

    xhr.open("POST", "/Shifts/Assign?shiftId=" + shiftId + "&assign=" + availabilityId);
    xhr.send();
}

function selectUser() {
    var assignWindow = document.getElementById("assignWindow");
    var shiftId = assignWindow.getAttribute("attr-id");
    var select = document.getElementById("selectUser");
    var user = select.options[select.selectedIndex].value;

    assignUser(user, shiftId);
}

function assignUser(username, shiftId) {
    var xhr = new XMLHttpRequest();

    xhr.addEventListener("load", function () {
        if (this.status == 200) {
            reload("Shifts");
        } else {
            alert("Error " + this.status);
        }
    });

    xhr.responseType = "json";

    xhr.open("POST", "/Shifts/Assign?shiftId=" + shiftId + "&assign=" + username);
    xhr.send();
}

function assignRecurringly(availabilityId, shiftId, availableWeeks) {
    var xhr = new XMLHttpRequest();

    xhr.addEventListener("load", function () {
        if (this.status == 200) {
            // reload("Shifts");
        } else {
            alert("Error " + this.status);
        }
    });

    xhr.responseType = "json";

    xhr.open("POST", "/Shifts/AssignRecurringly?shiftId=" + shiftId + "&availabilityId=" + availabilityId + "&availableWeeks=" + availableWeeks);
    xhr.send();
}

function unAssign(shiftId) {
    var xhr = new XMLHttpRequest();

    xhr.addEventListener("load", function () {
        if (this.status == 200) {
            reload("Shifts")
        } else {
            alert("Error " + this.status);
        }
    });

    xhr.responseType = "json";

    xhr.open("POST", "/Shifts/UnAssign?shiftId=" + shiftId);
    xhr.send();
}
