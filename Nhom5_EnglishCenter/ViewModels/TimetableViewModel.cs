using TrungTamAnhNgu.Web.Models;

namespace TrungTamAnhNgu.Web.ViewModels
{
    public class ClassSession
    {
        public Class ClassInfo { get; set; }
        public DateTime Date { get; set; } // Ngày học cụ thể
        public int ShiftIndex { get; set; } // Ca học (0-3)
    }

    public class TimetableViewModel
    {
        // Ngày bắt đầu của tuần đang xem
        public DateTime StartOfWeek { get; set; }

        // Ngày kết thúc của tuần đang xem
        public DateTime EndOfWeek { get; set; }

        // Ma trận phiên học [Ca, Thứ (0-6)]
        public List<ClassSession>[,] Grid { get; set; } = new List<ClassSession>[4, 7];

        public TimetableViewModel()
        {
            for (int ca = 0; ca < 4; ca++)
                for (int thu = 0; thu < 7; thu++)
                    Grid[ca, thu] = new List<ClassSession>();
        }
    }
}