// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

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

                $.ajax({
                    url: "/Availabilities/CreateDaily",
                    type: "POST",
                    data: values,
                    success: reload,
                    error: function (res) {
                        if (res.responseText == "overlap") {
                            alert("You already have an availability during these hours.")
                        }
                    }
                });
            } else if ($('input[name=dailyPattern]:checked').val() == "weekday") {
                values.Range = $("input[name=range]").val();

                $.ajax({
                    url: "/Availabilities/createDailyWeekdays",
                    type: "POST",
                    data: values,
                    success: reload,
                    error: function (res) {
                        if (res.responseText == "overlap") {
                            alert("You already have an availability during these hours.")
                        }
                    }
                });
            }
        } else if ($('input[name=pattern]:checked').val() == "weekly") {
            var days = $("input[name=everyWeeksDays]:checked").map(function () {
                return $(this).val();
            }).get();
            values.Pattern = $("input[name=everyWeeks]").val();
            values.Range = $("input[name=range]").val();
            values.Days = days;

            $.ajax({
                url: "/Availabilities/CreateWeekly",
                type: "POST",
                data: values,
                success: reload,
                error: function (res) {
                    if (res.responseText == "overlap") {
                        alert("You already have an availability during these hours.")
                    }
                }
            });
        }
    } else {
        $.ajax({
            url: "/Availabilities/Create",
            type: "POST",
            data: values,
            success: reload,
            error: function (res) {
                if (res.responseText == "overlap") {
                    alert("You already have an availability during these hours.")
                }
            }
        });
    }

    return false;
}

function reload() {
    location.reload();
}