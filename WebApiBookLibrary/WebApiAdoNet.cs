using Mysqlx.Crud;
using System.Text;

namespace WebApiBookLibrary
{
    public static class WebApiAdoNet
    {
        public static string GetUserInfo()
        {
            StringBuilder sb = new StringBuilder();
            //
            sb.Append("select user.LoginName, user.FullName, user.Discriminator from appusers as user ");
            sb.Append("where user.LoginName = @login ");
            //
            return sb.ToString();
        }
        public static string GetUsersByLoginBadBook(TypeQuery typeQuery)
        { 
            StringBuilder sb = new StringBuilder();
            //
            //user.LoginName, user.FullName, user.Discriminator, 
            sb.Append("select book.BiaId, book.Title, rent.TakenDate, rent.ExtendedDueDate, book.CoverImage ");
            sb.Append("from renthistories as rent ");
            sb.Append("join appusers as user ");
            sb.Append("on rent.UserUid = user.Uid ");
            sb.Append("join books as book ");
            sb.Append("on book.BiaId = rent.TakenBookBiaId ");
            sb.Append("where ");
            if(typeQuery == TypeQuery.BookByLogin)
                sb.Append("user.LoginName =  @login and ");
            sb.Append(" rent.ReturnedDate is null ");
            //
            return sb.ToString();
        }

        public static string GetAutorsById()
        {
            StringBuilder sb = new StringBuilder();
            //
            sb.Append("select auth.FirstName, auth.MiddleName, auth.LastName ");
            sb.Append("from books as book ");
            sb.Append("join bookauthors as bkAuth ");
            sb.Append("on bkAuth.BookId = book.BiaId ");
            sb.Append("join authors as auth ");
            sb.Append("on bkAuth.AuthorUid = auth.AuthorUid ");
            sb.Append("where book.BiaId = @bookId ");
            //
            return sb.ToString();
        }

        public static string GetRoleByUserLogin()
        {
            StringBuilder sb = new StringBuilder();
            //
            sb.Append("select user.Discriminator from appusers as user ");
            sb.Append("where user.LoginName = @login ");
            //
            return sb.ToString();
        }

        public static string GetCountReturnToDay()
        {
            StringBuilder sb = new StringBuilder();
            //
            sb.Append("select count(*) from renthistories as rent ");
            sb.Append("where rent.ExtendedDueDate = curdate() and rent.ReturnedDate is null ");
            //
            return sb.ToString();
        }

        public static string GetCountRequest()
        {
            StringBuilder sb = new StringBuilder();
            //
            sb.Append("select count(*) from requestbooks ");
            //
            return sb.ToString();
        }
    }
}
