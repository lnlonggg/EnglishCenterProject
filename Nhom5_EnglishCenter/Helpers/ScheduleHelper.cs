using TrungTamAnhNgu.Web.Models;
using TrungTamAnhNgu.Web.ViewModels;

namespace TrungTamAnhNgu.Web.Helpers
{
    public static class ScheduleHelper
    {
        public static TimetableViewModel ParseForWeek(IEnumerable<Class> classes, DateTime targetDate)
        {
            var model = new TimetableViewModel();

            // 1. Xác định ngày đầu tuần (Thứ 2) và cuối tuần (CN) của tuần targetDate
            int diff = (7 + (targetDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            model.StartOfWeek = targetDate.Date.AddDays(-1 * diff);
            model.EndOfWeek = model.StartOfWeek.AddDays(6);

            // 2. Duyệt từng ngày trong tuần đó (0=Thứ 2, ..., 6=CN)
            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                DateTime currentDayDate = model.StartOfWeek.AddDays(dayIndex);

                // Chuyển DayIndex sang string trong DB (T2, T3...)
                string dayCode = GetDayCode(dayIndex);

                foreach (var lop in classes)
                {
                    // KIỂM TRA 1: Lớp này có hoạt động vào ngày này không?
                    // (Phải nằm trong khoảng StartDate và EndDate)
                    if (currentDayDate < lop.StartDate || currentDayDate > lop.EndDate)
                    {
                        continue; // Lớp chưa mở hoặc đã đóng vào ngày này
                    }

                    // KIỂM TRA 2: Lớp này có lịch vào thứ này không?
                    if (string.IsNullOrEmpty(lop.Schedule)) continue;
                    var parts = lop.Schedule.Split('|');
                    if (parts.Length < 2) continue;

                    var daysPart = parts[0].Trim(); // "T2, T4"
                    if (!daysPart.Contains(dayCode))
                    {
                        continue; // Lớp này không học vào thứ này
                    }

                    // 3. Xác định Ca
                    string shiftPart = parts[1].Trim();
                    int shiftIndex = GetShiftIndex(shiftPart);

                    if (shiftIndex != -1)
                    {
                        model.Grid[shiftIndex, dayIndex].Add(new ClassSession
                        {
                            ClassInfo = lop,
                            Date = currentDayDate,
                            ShiftIndex = shiftIndex
                        });
                    }
                }
            }

            return model;
        }

        private static string GetDayCode(int index)
        {
            return index switch
            {
                0 => "T2",
                1 => "T3",
                2 => "T4",
                3 => "T5",
                4 => "T6",
                5 => "T7",
                6 => "CN",
                _ => ""
            };
        }

        private static int GetShiftIndex(string shiftPart)
        {
            if (shiftPart.StartsWith("Ca 1")) return 0;
            if (shiftPart.StartsWith("Ca 2")) return 1;
            if (shiftPart.StartsWith("Ca 3")) return 2;
            if (shiftPart.StartsWith("Ca 4")) return 3;
            return -1;
        }
    }
}