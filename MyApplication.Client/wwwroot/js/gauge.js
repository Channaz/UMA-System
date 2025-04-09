document.addEventListener("DOMContentLoaded", function () {
    //initializeGauge();
    updateGaugeValue(value);
});

if (!Array.prototype.forEach) {
    Array.prototype.forEach = function (cb) {
        var i = 0, s = this.length;
        for (; i < s; i++) {
            cb && cb(this[i], i, this);
        }
    };
}

function initializeGauge() {
    let gaugeCanvas = document.querySelector("#gaugeCanvas");

    if (!gaugeCanvas) {
        console.warn("Gauge canvas not found! Retrying in 500ms...");
        setTimeout(initializeGauge, 500); // Retry after 500ms
        return;
    }

    if (!gaugeCanvas.gauge) {
        console.log("Initializing gauge...");
        gaugeCanvas.gauge = new RadialGauge({
            renderTo: gaugeCanvas,
            minValue: 0,
            maxValue: 100,
        }).draw();
    }
}

document.fonts && document.fonts.forEach(function (font) {
    font.loaded.then(function () {
        if (font.family.match(/Led/)) {
            document.gauges.forEach(function (gauge) {
                gauge.update();
            });
        }
    });
});

//var timers = [];

//function animateGauges() {
//    document.gauges.forEach(function (gauge) {
//        timers.push(setInterval(function () {
//            var min = gauge.options.minValue - 20;
//            var max = gauge.options.maxValue + 20;

//            gauge.value = min + Math.random() * (max - min);
//        }, gauge.animation.duration + 50));
//    });
//}

//function stopGaugesAnimation() {
//    timers.forEach(function (timer) {
//        clearInterval(timer);
//    });
//    timers = [];
//}

function updateGaugeValue(value) {
    let gaugeCanvas = document.querySelector("#gaugeCanvas");

    //if (!gaugeCanvas || !gaugeCanvas.gauge) {
    //    console.warn("Gauge is not initialized yet. Retrying...");
    //    setTimeout(() => updateGaugeValue(value), 500); // Retry after 500ms
    //    return;
    //}

    let gauge = gaugeCanvas.gauge;
    gauge.value = value;
}