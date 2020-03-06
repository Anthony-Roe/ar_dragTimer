$(function () {
    resourceName = GetParentResourceName();

    News = {};
    News.articles = [];

    mainScreen = $("#window");
    curTime = $("#curTime");
    bestTime = $("#bestTime");
    distance = $("#distance");
    distanceBorder = $("#distanceBorder");
    distanceProgress = $("#distanceProgress")
    goal = $("#goal");
    statusBar = $("#statusBar");

    mainScreen.hide();

    window.addEventListener('message', function (event) {
        var data = event.data;

        if (data.menuState != null)
        {
            if (data.menuState === true)
                mainScreen.fadeIn(100);
            else
                mainScreen.fadeOut(100);
        }
        if (data.setData != null) {
            curTime.html("Time: " + data.setData.curTime.toFixed(2));
            bestTime.html("Best Time: " + data.setData.bestTime.toFixed(2));
            distance.html("Distance(Meters): " + data.setData.distance.toFixed(1) + "/" + data.setData.goal);
            distanceProgress.width((data.setData.distance.toFixed() / data.setData.goal.toFixed()) * 100 + "%");
            statusBar.css("background-color", data.setData.statusColor);
        }
    });
})