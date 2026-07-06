
function startVideo(src) {
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        navigator.mediaDevices.getUserMedia({ video: true }).then(function (stream) {
            let video = document.getElementById(src);
            if ("srcObject" in video) {
                video.srcObject = stream;
            } else {
                video.src = window.URL.createObjectURL(stream);
            }
            video.onloadedmetadata = function (e) {
                video.play();
            };
            //mirror image
            video.style.webkitTransform = "scaleX(-1)";
            video.style.transform = "scaleX(-1)";
        });
    }
}

function getFrame(src, dest) {
    let video = document.getElementById(src);
    let canvas = document.getElementById(dest);
    canvas.getContext('2d').drawImage(video, 0, 0, 320, 240);
}

function getImageFrame(src, dest, dotNetHelper) {
    let video = document.getElementById(src);
    let canvas = document.getElementById(dest);
    canvas.getContext('2d').drawImage(video, 0, 0, 320, 240);

    let dataUrl = canvas.toDataURL("image/jpeg");
    dotNetHelper.invokeMethodAsync('ProcessImage', dataUrl);
}

function stopVideo(src) {
    let video = document.getElementById(src);
    if (video && "srcObject" in video) {
        const stream = video.srcObject;
        if (stream) {
            const tracks = stream.getTracks();
            tracks.forEach(track => track.stop());
        }
        video.srcObject = null;
    }
}

function JSLoadPatientsByMCenter(repUrl) {
    var report = {
        "dataSource": {
            "dataSourceType": "json",
            "filename": repUrl
        },
        "slice": {
            "rows": [
                {
                    "uniqueName": "Мед. центр"
                },
                {
                    "uniqueName": "Пациент"
                }
            ],
            "columns": [
                {
                    "uniqueName": "Measures"
                }
            ],
            "measures": [
                {
                    "uniqueName": "Сеансы",
                    "aggregation": "sum"
                },
                {
                    "uniqueName": "inn",
                    "aggregation": "stdevs",
                    "active": false
                }
            ],
            "expands": {
                "rows": [
                    {
                        "tuple": [
                            "medCenterTitle"
                        ]
                    }
                ]
            }
        },
        "tableSizes": {
            "columns": [
                {
                    "idx": 0,
                    "width": 244
                }
            ]
        }
    }
    pivot.setReport(report);
}