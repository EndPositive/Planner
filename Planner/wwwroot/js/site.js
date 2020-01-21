// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function PostAvailabilities(endpoint, type, values) {
    $.ajax({
        url: "/Availabilities/" + endpoint,
        type: type,
        data: values,
        success: reload,
        error: function (res) {
            if (res.responseText == "overlap") {
                alert("You already have an availability during these hours.")
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
    var id = $("input[name=Id]").val();
    var values = {
        StartTime: $("input[name=StartTime").val(),
        EndTime: $("input[name=EndTime]").val(),
    };
    if ($('#enableSeries').is(':checked')) {
        PostAvailabilities("EditSeries/"+id, "POST", values);
    } else {
        values.Date = $("input[name=Date]").val();
        PostAvailabilities("Edit/" + id, "POST", values);
    }

    return false;
}

function reload() {
    window.location.href = "/Availabilities";
}