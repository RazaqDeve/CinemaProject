using System;
using System.ComponentModel;


namespace CatalogService.Entities.Enums
{
    public enum Genre
    {
        [Description("اکشن")]
        Action,

        [Description("ماجراجویی")]
        Adventure,

        [Description("انیمیشن")]
        Animation,

        [Description("کمدی")]
        Comedy,

        [Description("جنایی")]
        Crime,

        [Description("مستند")]
        Documentary,

        [Description("درام")]
        Drama,

        [Description("خانوادگی")]
        Family,

        [Description("فانتزی")]
        Fantasy,

        [Description("تاریخی")]
        History,

        [Description("ترسناک")]
        Horror,

        [Description("موزیکال")]
        Music,

        [Description("معمایی")]
        Mystery,

        [Description("عاشقانه")]
        Romance,

        [Description("علمی-تخیلی")]
        ScienceFiction,

        [Description("دلهره‌آور")]
        Thriller,

        [Description("جنگی")]
        War,

        [Description("وسترن")]
        Western
    }
}