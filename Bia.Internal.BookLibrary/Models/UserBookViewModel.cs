using Bia.Internal.BookLibrary.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bia.Internal.BookLibrary.Models
{
    public class UserBookViewModel
    {
        [Display(Name = "Isbn")]
        public string Isbn { get; set; }

        [Display(Name = "№")]
        public int BiaId { get; set; }

        [Display(Name = "Название")]
        public string Title { get; set; }

        [Display(Name = "Еще название")]
        public string Subtitle { get; set; }

        [Display(Name = "Язык")]
        public LanguageId Language { get; set; }

        [Display(Name = "Авторы")]
        public ICollection<AuthorViewModel> Authors { get; set; }

        [Display(Name = "Категории")]
        public ICollection<CategoryViewModel> Cathegories { get; set; }

        [Display(Name = "Отзывы")]
        public ICollection<ReviewViewModel> Reviews { get; set; }

        [Display(Name = "Год")]
        public int? Year { get; set; }

        [Display(Name = "Издание")]
        public int? Edition { get; set; }

        [Display(Name = "Стр.")]
        public int? Pages { get; set; }

        [Display(Name = "Обложка")]
        public string CoverImage { get; set; }

        [Display(Name = "Описание")]
        public string Annotation { get; set; }

        [Display(Name = "Рейтинг")]
        public int AverageRating { get; set; }

    }
}
