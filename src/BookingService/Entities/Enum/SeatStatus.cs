using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Entities.Entity
{
    // وضعیت صندلی در یک سانس.
    public enum SeatStatus
    {
        Available = 1,   // آزاد و قابل انتخاب
        Purchased = 2,   // خریداری شده
        Selecting = 3,   // موقتاً در اختیار مشتری برای خرید
        Reserved = 4,    // کنار گذاشته شده توسط مدیر سینما
        Broken = 5       // خراب و غیرقابل استفاده
    }
}