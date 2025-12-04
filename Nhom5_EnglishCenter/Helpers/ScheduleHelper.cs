using TrungTamAnhNgu.Web.Models;
using TrungTamAnhNgu.Web.ViewModels;
using System.Text.RegularExpressions;

namespace TrungTamAnhNgu.Web.Helpers
{
    public static class ScheduleHelper
    {
        // Cấu trúc mới để lưu 1 slot học
        public class ClassSlot
        {
            public string Day { get; set; } // T2, T3...
            public int Shift { get; set; }  // 1, 2, 3, 4
        }

        // Hàm phân tích chuỗi mới: "T2 (Ca 1, Ca 2); T4 (Ca 1)"
        public static List<ClassSlot> ParseString(string scheduleStr)
        {
            var list = new List<ClassSlot>();
            if (string.IsNullOrEmpty(scheduleStr)) return list;

            // Tách theo dấu chấm phẩy để lấy từng ngày: ["T2 (Ca 1, Ca 2)", "T4 (Ca 1)"]
            var dayParts = scheduleStr.Split(';');

            foreach (var part in dayParts)
            {
                // Dùng Regex để bóc tách: "T2" và "1, 2"
                // Pattern: Lấy chữ cái đầu (Day) và nội dung trong ngoặc (Shifts)
                var match = Regex.Match(part.Trim(), @"^(T\d|CN)\s*\((.*)\)$");
                if (match.Success)
                {
                    string day = match.Groups[1].Value;
                    string shiftsStr = match.Groups[2].Value; // "Ca 1, Ca 2"

                    var shifts = shiftsStr.Split(',');
                    foreach (var s in shifts)
                    {
                        // Lấy số từ "Ca 1"
                        var numberMatch = Regex.Match(s, @"\d+");
                        if (numberMatch.Success)
                        {
                            list.Add(new ClassSlot { Day = day, Shift = int.Parse(numberMatch.Value) });
                        }
                    }
                }
            }
            return list;
        }

        public static TimetableViewModel ParseForWeek(IEnumerable<Class> classes, DateTime targetDate)
        {
            var model = new TimetableViewModel();

            // Xác định ngày đầu tuần
            int diff = (7 + (targetDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            model.StartOfWeek = targetDate.Date.AddDays(-1 * diff);
            model.EndOfWeek = model.StartOfWeek.AddDays(6);

            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                DateTime currentDayDate = model.StartOfWeek.AddDays(dayIndex);
                string dayCode = dayIndex == 6 ? "CN" : $"T{dayIndex + 2}";

                foreach (var lop in classes)
                {
                    if (currentDayDate < lop.StartDate || currentDayDate > lop.EndDate) continue;

                    // Phân tích lịch học linh hoạt
                    var slots = ParseString(lop.Schedule);

                    // Kiểm tra xem lớp này có slot nào trùng với ngày (dayCode) hiện tại không
                    foreach (var slot in slots)
                    {
                        if (slot.Day == dayCode)
                        {
                            // Shift 1 -> Index 0
                            int shiftIdx = slot.Shift - 1;
                            if (shiftIdx >= 0 && shiftIdx < 4)
                            {
                                model.Grid[shiftIdx, dayIndex].Add(new ClassSession
                                {
                                    ClassInfo = lop,
                                    Date = currentDayDate,
                                    ShiftIndex = shiftIdx
                                });
                            }
                        }
                    }
                }
            }
            return model;
        }
    }
}