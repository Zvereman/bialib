using Microsoft.AspNetCore.Authorization;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Policy;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace WebApiBookLibrary
{
    public enum TypeQuery
    {
        AllBook = 0,
        BookByLogin = 1,
    }

    public class AdminInfo
    {
        public int CountReturnToDay { get; set; }
        public int RequestToDay { get; set; }
        public AdminInfo()
        {
            this.CountReturnToDay = 0;
            this.RequestToDay = 0;
        }

        public AdminInfo(int countReturnToDay, int requestToDay)
        {
            this.CountReturnToDay = countReturnToDay;
            this.RequestToDay = requestToDay;
        }

        public AdminInfo? Load(string strConn, string login)
        {
            AdminInfo? result = new AdminInfo();
            int countReturnToDay = 0;
            int countRequestToDay = 0;
            string role = string.Empty;
            //
            if (string.IsNullOrWhiteSpace(login))
                throw new Exception("AdminInfo.Load: Ожидалось значение (string)login");
            if (string.IsNullOrWhiteSpace(strConn))
                throw new Exception("AdminInfo.Load: Ожидалось значение (string)strConn");
            //
            using (MySqlConnection conn = new MySqlConnection(strConn))
            {
                conn.Open();
                if (conn.State == ConnectionState.Open)
                {

                    using (MySqlCommand cmd = new MySqlCommand(WebApiAdoNet.GetRoleByUserLogin(), conn))
                    {
                        cmd.Parameters.Add(new MySqlParameter("@login", login));
                        //
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader.GetValue(0) == null)
                                    throw new Exception("AdminInfo.Load: Ожидалось значение (string)Discriminator");
                                //
                                role = reader.GetString(0);
                            }
                        }
                    }
                    //
                    if (role == "Admin")
                    {
                        using (MySqlCommand cmdReturn = new MySqlCommand(WebApiAdoNet.GetCountReturnToDay(), conn))
                        {
                            using (MySqlDataReader reader = cmdReturn.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if (reader.GetValue(0) == null)
                                        throw new Exception("AdminInfo.Load: Ожидалось значение (string)CountReturnToDay");
                                    //
                                    countReturnToDay = reader.GetInt32(0);
                                }
                            }
                        }
                        //
                        using (MySqlCommand cmdRequest = new MySqlCommand(WebApiAdoNet.GetCountRequest(), conn))
                        {
                            using (MySqlDataReader reader = cmdRequest.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if (reader.GetValue(0) == null)
                                        throw new Exception("AdminInfo.Load: Ожидалось значение (string)CountRequestToDay");
                                    //
                                    countRequestToDay = reader.GetInt32(0);
                                }
                            }
                        }
                        //
                        result = new AdminInfo(countReturnToDay, countRequestToDay);
                    }
                    else
                    {
                        result = null;
                    }
                }
            }
            //
            return result;
        }
    }

    public class Autors
    {
        public string FirstNameAutors { get; set; }
        public string MiddleNameAutors { get; set; }
        public string LastNameAutors { get; set; }

        public Autors()
        {
            this.FirstNameAutors = string.Empty;
            this.MiddleNameAutors = string.Empty;
            this.LastNameAutors = string.Empty;
        }

        public Autors(string firstNameAutors, string middleNameAutors, string lastNameAutors)
        {
            this.FirstNameAutors = firstNameAutors;
            this.MiddleNameAutors = middleNameAutors;
            this.LastNameAutors = lastNameAutors;
        }
    }

    public class UserDetails
    {
        public string Login { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }

        public UserDetails()
        {
            this.Login = string.Empty;
            this.FullName = string.Empty;
            this.Role = string.Empty;
        }

        public UserDetails(string login, string fullName, string role)
        {
            this.Login = login;
            this.FullName = fullName;
            this.Role = role;
        }
    }

    public class GetBadBooksByUserLogin
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public DateTime TakenDate { get; set; }
        public DateTime ExtendedDueDate { get; set; }
        public List<Autors> AutorsForBook { get; set; }
        //public byte[] CoverImage { get; set; }
        public string CoverImage { get; set; }

        public GetBadBooksByUserLogin()
        {
            this.BookId = 0;
            this.Title = string.Empty;
            this.TakenDate = new DateTime();
            this.ExtendedDueDate = new DateTime();
            this.AutorsForBook = new List<Autors>();
            //this.CoverImage = new byte[0];
            this.CoverImage = string.Empty;
        }

        //public GetBadBooksByUserLogin(int bookId, string title, DateTime takenDate,
        //    DateTime extendedDueDate, List<Autors> autors, byte[] coverImage)
        public GetBadBooksByUserLogin(int bookId, string title, DateTime takenDate,
            DateTime extendedDueDate, List<Autors> autors, string coverImage)
        {
            this.BookId = bookId;
            this.Title = title;
            this.TakenDate = takenDate;
            this.ExtendedDueDate = extendedDueDate;
            this.AutorsForBook = autors;
            this.CoverImage = coverImage;
        }
    }

    public class UserInfo
    {
        public UserDetails UserDetails { get; set; }
        public List<GetBadBooksByUserLogin> BadBooksByUser { get; set; }

        public UserInfo()
        {
            this.UserDetails = new UserDetails();
            this.BadBooksByUser = new List<GetBadBooksByUserLogin>();
        }

        public UserInfo(UserDetails userDetails, List<GetBadBooksByUserLogin> badBooksByUsers)
        {
            this.UserDetails = userDetails;
            this.BadBooksByUser = badBooksByUsers;
        }

        public async Task<UserInfo> GetUserInfo(string strConn, string login, string libraryUrl)
        {
            UserInfo result = new UserInfo();
            UserDetails userDetails = new UserDetails();
            List<GetBadBooksByUserLogin> badBookSexUserByLogins = new List<GetBadBooksByUserLogin>();
            //
            if (string.IsNullOrWhiteSpace(login))
                throw new Exception("UserInfo.GetUserInfo: Ожидалось значение (string)login");
            if (string.IsNullOrWhiteSpace(strConn))
                throw new Exception("UserInfo.GetUserInfo: Ожидалось значение (string)strConn");
            //
            using (MySqlConnection conn = new MySqlConnection(strConn))
            {
                conn.Open();
                if (conn.State == ConnectionState.Open)
                {
                    using (MySqlCommand cmd = new MySqlCommand(WebApiAdoNet.GetUserInfo(), conn))
                    {
                        cmd.Parameters.Add(new MySqlParameter("@login", login));
                        //
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            string loginFromDb;
                            string fullName;
                            string role;
                            while (reader.Read())
                            {
                                loginFromDb = reader.GetValue(0) as string ?? string.Empty;
                                fullName = reader.GetValue(1) as string ?? string.Empty;
                                role = reader.GetValue(2) as string ?? string.Empty;
                                //
                                userDetails = new UserDetails(loginFromDb, fullName, role);

                            }
                        }
                    }
                    //
                    using (MySqlCommand cmd = new MySqlCommand(WebApiAdoNet.GetUsersByLoginBadBook(TypeQuery.BookByLogin), conn))
                    {
                        cmd.Parameters.Add(new MySqlParameter("@login", login));
                        //
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            int bookId;
                            string title;
                            DateTime takenDate;
                            DateTime extendedDueDate;

                            string? coverImageUrl;
                            //byte[] coverImage;
                            while (reader.Read())
                            {
                                bookId = reader.GetValue(0) is int ? reader.GetInt32(0) : 0;
                                title = reader.GetValue(1) as string ?? string.Empty;
                                takenDate = reader.GetValue(2) is DateTime ? reader.GetDateTime(2) : DateTime.MinValue;
                                extendedDueDate = reader.GetValue(3) is DateTime ? reader.GetDateTime(3) : DateTime.MinValue;

                                coverImageUrl = reader.GetValue(4) as string ?? string.Empty;
                                if(!string.IsNullOrEmpty(coverImageUrl))
                                    coverImageUrl = ($"{libraryUrl}{coverImageUrl}");
                                //coverImage = await GetBookCoverInByte($"{libraryUrl}{coverImageUrl}");
                                //
                                GetBadBooksByUserLogin getBadBookSexUserByLogin = new GetBadBooksByUserLogin(
                                    bookId, title, takenDate, extendedDueDate, null, coverImageUrl);


                                badBookSexUserByLogins.Add(getBadBookSexUserByLogin);
                            }
                        }
                    }
                    //
                    foreach (GetBadBooksByUserLogin item in badBookSexUserByLogins)
                    {
                        List<Autors> autorsList = new List<Autors>();
                        //
                        using (MySqlCommand cmd = new MySqlCommand(WebApiAdoNet.GetAutorsById(), conn))
                        {
                            cmd.Parameters.Add(new MySqlParameter("@bookId", item.BookId));
                            //
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                string firstName;
                                string middleName;
                                string lastName;
                                while (reader.Read())
                                {
                                    firstName = reader.GetValue(0) as string ?? string.Empty;
                                    middleName = reader.GetValue(1) as string ?? string.Empty;
                                    lastName = reader.GetValue(2) as string ?? string.Empty;

                                    Autors autors = new Autors(firstName, middleName, lastName);

                                    autorsList.Add(autors);
                                }
                            }

                            item.AutorsForBook = autorsList;
                        }
                    }
                }
                else
                    throw new Exception(string.Format("UserInfo.GetUserInfo: Ошибка подключения к БД. Статус подключения: \"{0}\"",
                        conn.State.ToString()));
            }
            //
            result = new UserInfo(userDetails, badBookSexUserByLogins);
            //
            return result;
        }
        //protected async Task<byte[]> GetBookCoverInByte(string url)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(url))
        //            return new byte[0];
        //        //
        //        using (HttpClient client = new HttpClient())
        //        {
        //            HttpResponseMessage response = await client.GetAsync(url);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
        //                return imageBytes;
        //            }
        //            else
        //            {
        //                Console.WriteLine($"Ошибка загрузки изображения. Статус код: {response.StatusCode}");
        //                return new byte[0];
        //            }
        //        }

        //    }
        //    catch (HttpRequestException)
        //    {
        //        return new byte[0];
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Ошибка при загрузке изображения: {ex.Message}");
        //    }
        //}
    }
}
