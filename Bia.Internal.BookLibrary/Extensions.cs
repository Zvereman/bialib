using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bia.Internal.BookLibrary.Data;
using Bia.Internal.BookLibrary.Models;

namespace Bia.Internal.BookLibrary
{
    public static class Extensions
    {
        public static IEnumerable<RentedBook> ToRentedBooks(this IEnumerable<PaperBook> books)
        {
            var result = new List<RentedBook>();
            foreach (var book in books)
            {
                var rent = book.RentHistory.OrderBy(q=>q.TakenDate).First();
                result.Add(new RentedBook()
                {
                    BiaId = book.BiaId,
                    Title = book.Title,
                    User = book.TakenByUser?.LoginName,
                    EndRenting = book.TakenDue.GetValueOrDefault(),
                    StartRenting = rent.TakenDate,
                });
            }

            return result;
        }

        public static string GenerateLink(this string str)
        {
            //if (Uri.IsWellFormedUriString(str, UriKind.Absolute))
            //{
            //    return str;
            //}
            return str.Split('\\').Last();
        }

        public static UserBookViewModel ToUserBookViewModel(this Book book)
        {
            var result = new UserBookViewModel()
            {
                Isbn = book.Isbn,
                BiaId = book.BiaId,
                Title = book.Title,
                Subtitle = book.Subtitle,
                Language = book.Language,
                Year = book.Year,
                Edition = book.Edition,
                Pages = book.Pages,
                CoverImage = book.CoverImage,
                Annotation = book.Annotation,
            };
            result.Authors = new List<AuthorViewModel>();
            foreach (var author in book.Authors)
            {
                result.Authors.Add(new AuthorViewModel()
                {
                    
                });
            }
            result.Cathegories = new List<CategoryViewModel>();
            result.Reviews = new List<ReviewViewModel>();

            return result;
        }

        public static int SumCurrentRating(this Book book)
        {
            var rating = book.Reviews.Select(q => q.Rating);
            var curr = rating.Sum();
            var count = rating.Count();
            curr = (curr > 0) ? curr/count : 0 ;
            return curr;
        }

        public static string FullName(this Author author)
        {
            if (string.IsNullOrEmpty(author.MiddleName))
            {
                return $"{(!string.IsNullOrEmpty(author.LastName) ? char.ToUpper(author.LastName[0]) + author.LastName.Substring(1).ToLower() : null)}" +
                    $" {(!string.IsNullOrEmpty(author.FirstName) ? char.ToUpper(author.FirstName[0]) + author.FirstName.Substring(1).ToLower() : null)}";
            }
            return $"{(!string.IsNullOrEmpty(author.LastName) ? char.ToUpper(author.LastName[0]) + author.LastName.Substring(1).ToLower() : null)} {(!string.IsNullOrEmpty(author.FirstName) ? char.ToUpper(author.FirstName[0]) + author.FirstName.Substring(1).ToLower() : null)} {(!string.IsNullOrEmpty(author.MiddleName) ? char.ToUpper(author.MiddleName[0]) + author.MiddleName.Substring(1).ToLower() : null)}";
        }

        public static string Initials(this Author author)
        {
            if (string.IsNullOrEmpty(author.MiddleName))
            {
                return $"{author.FirstName[0]}. {author.LastName}";
            }
            return $"{author.FirstName[0]}.{author.MiddleName[0]}. {author.LastName}";
        }

    }
}
