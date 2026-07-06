window.ReportingViewerCustomization = {
    onCustomizeParameterEditors: function (s, e) {
        //Remove time part from the DateTime parameter editor
        if (e.parameter.type === "System.DateTime") {
            e.info.editor = $.extend({}, e.info.editor);
            e.info.editor.extendedOptions = $.extend(e.info.editor.extendedOptions || {}, { type: 'date' });
        }
    },

    onCustomizeLocalization: function (s, e) {
        s.UpdateLocalization({
            'Properties': 'Характеристики',          
            'Search': 'Поиск',
            'Export To': 'Экспортировать',
            'Match case': 'Соответствовать',
            'Match whole word only': 'Совпадение только с целым словом',
            'Full Screen': 'Полноэкранный',
            'Print': 'Распечатать',
            'Print Page': 'Распечатать страницу',
            'Zoom Out': 'Уменьшить',
            'Zoom In': 'Приблизить',
            'Toggle Multipage Mode':'Переключить многостраничный режим'
        });
    },
    //Change default Zoom level
    onBeforeRender: function (s, e) {
        //-1: Page Width
        //0: Whole Page
        //1: 100%
        e.reportPreview.zoom(1);
    }
}