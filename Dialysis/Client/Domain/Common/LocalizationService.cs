using DevExpress.Blazor.Localization;
using System.Globalization;

namespace Dialysis.Client.Domain.Common
{
  
        public class LocalizationService : DxLocalizationService, IDxLocalizationService
        {
            string? IDxLocalizationService.GetString(string key)
            {
                return CultureInfo.CurrentUICulture.Name == "ru-RU" ?
                    LocalizationProvider.GetString(key) :
                    base.GetString(key);
            }

            public static class LocalizationProvider
            {
                public static Dictionary<string, string> localization = new Dictionary<string, string> {
                {"DxBlazorStringId.Grid_Summary_Count", "Всего"},
                {"DxBlazorStringId.Grid_ColumnChooser_HeaderText", "Выберите столбцы, которые хотите увидеть"},
                {"DxBlazorStringId.Grid_GroupPanel", "Пожалуйста, перетащите сюда столбцы, которые вы хотите сгруппировать"},
                {"DxBlazorStringId.Grid_Loading", "Loading"},
                {"DxBlazorStringId.Grid_Summary_Sum", "Итог"},
                {"DxBlazorStringId.Grid_Summary_SumOfAnotherColumn", "Итог"},
                {"DxBlazorStringId.Grid_Summary_Min", "Мин"},
                {"DxBlazorStringId.Grid_Summary_Max", "Макс"},
                {"DxBlazorStringId.Grid_Summary_Average", "Среднее"},
                {"DxBlazorStringId.Grid_PageSizeSelector_AllRowsItem", "Все"},
                {"DxBlazorStringId.Grid_PageSizeSelector_Caption", "Показывать"},
               
            };

                public static string? GetString(string key)
                {
                    localization.TryGetValue(key, out string? value);
                    return value;
                }
            }
        }
    
}
