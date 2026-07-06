function loadChart(src) {

	//Better to construct options first and then pass it as a parameter
	var options = {
		animationEnabled: true,
		theme: "light2",
		title: {
			text: "ОТЧЕТ ПО ПОЛИСУ ОМС ЗА 2022 ГОДА"
		},
		axisY2: {
			prefix: "",
			lineThickness: 0
		},
		toolTip: {
			shared: true
		},
		legend: {
			verticalAlign: "top",
			horizontalAlign: "center"
		},
		data: [
			{
				type: "stackedBar",
				showInLegend: true,
				name: "Лица самостоятельно уплачивающие взносы на ОМС",
				axisYType: "secondary",
				color: "#7E8F74",
				dataPoints: [
					{ y: 23, label: "Бишкек" },
					{ y: 14, label: "Чуй" },
					{ y: 30, label: "Баткен" },
					{ y: 25, label: "Ош" },
					{ y: 15, label: "Нарын" },
					{ y: 25, label: "Талас" },
					{ y: 29, label: "Жалал-Абад" },
					{ y: 12, label: "Иссык-Куль" },					
				]
			}
		]
	};

	$("#chartContainer").CanvasJSChart(options);
}


function loadCake(src) {

	var options = {
		title: {
			text: "ИНФОРМАЦИЯ ПО ГЕМОДИАЛИЗУ"
		},
		data: [{
			type: "pie",
			startAngle: 45,
			showInLegend: "true",
			legendText: "{label}",
			indexLabel: "{label} ({y})",
			yValueFormatString: "#,##0.#" % "",
			dataPoints: [
				{ label: "г.Бишкек", y: 36 },
				{ label: "г.Ош и Ошская область", y: 31 },
				{ label: "Жалал-Абадская область", y: 7 },
				{ label: "Нарынская область", y: 3 },
				{ label: "Баткенская область", y: 6 },
				{ label: "Таласская область", y: 4 },
				{ label: "Иссык-Кульская область", y: 3 },
				{ label: "Чуйская область", y: 5 }
			]
		}]
	};
	$("#chartContainerCake").CanvasJSChart(options);

}