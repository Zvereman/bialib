using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bia.Internal.BookLibrary.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Uid = table.Column<Guid>(nullable: false),
                    LoginName = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    FullName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    DateJoined = table.Column<DateTime>(nullable: false),
                    AccessGroup = table.Column<int>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Uid);
                });

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    AuthorUid = table.Column<Guid>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    MiddleName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.AuthorUid);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CategoryName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "AdminNotifies",
                columns: table => new
                {
                    NotifyId = table.Column<int>(nullable: false),
                    Uid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminNotifies", x => x.NotifyId);
                    table.ForeignKey(
                        name: "FK_AdminNotifies_AppUsers_Uid",
                        column: x => x.Uid,
                        principalTable: "AppUsers",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Arrangements",
                columns: table => new
                {
                    AppUserUid = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arrangements", x => x.AppUserUid);
                    table.ForeignKey(
                        name: "FK_Arrangements_AppUsers_AppUserUid",
                        column: x => x.AppUserUid,
                        principalTable: "AppUsers",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    BiaId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Isbn = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Subtitle = table.Column<string>(nullable: true),
                    Language = table.Column<int>(nullable: false),
                    Year = table.Column<int>(nullable: true),
                    Edition = table.Column<int>(nullable: true),
                    Pages = table.Column<int>(nullable: true),
                    CoverImage = table.Column<string>(nullable: true),
                    Annotation = table.Column<string>(nullable: true),
                    AverageRating = table.Column<int>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    EbookFormat = table.Column<int>(nullable: true),
                    Size = table.Column<int>(nullable: true),
                    Link = table.Column<string>(nullable: true),
                    TakenDue = table.Column<DateTime>(nullable: true),
                    TakenByUserUid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.BiaId);
                    table.ForeignKey(
                        name: "FK_Books_AppUsers_TakenByUserUid",
                        column: x => x.TakenByUserUid,
                        principalTable: "AppUsers",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IgnoredUsers",
                columns: table => new
                {
                    AppUserUid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgnoredUsers", x => x.AppUserUid);
                    table.ForeignKey(
                        name: "FK_IgnoredUsers_AppUsers_AppUserUid",
                        column: x => x.AppUserUid,
                        principalTable: "AppUsers",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookAuthors",
                columns: table => new
                {
                    BookId = table.Column<int>(nullable: false),
                    AuthorUid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookAuthors", x => new { x.BookId, x.AuthorUid });
                    table.ForeignKey(
                        name: "FK_BookAuthors_Authors_AuthorUid",
                        column: x => x.AuthorUid,
                        principalTable: "Authors",
                        principalColumn: "AuthorUid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookAuthors_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BiaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookCategories",
                columns: table => new
                {
                    BookId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCategories", x => new { x.BookId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_BookCategories_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BiaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DownloadHistories",
                columns: table => new
                {
                    Uid = table.Column<Guid>(nullable: false),
                    DownloadDate = table.Column<DateTime>(nullable: false),
                    UserUid = table.Column<Guid>(nullable: false),
                    DownloadedBookBiaId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadHistories", x => x.Uid);
                    table.ForeignKey(
                        name: "FK_DownloadHistories_Books_DownloadedBookBiaId",
                        column: x => x.DownloadedBookBiaId,
                        principalTable: "Books",
                        principalColumn: "BiaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DownloadHistories_AppUsers_UserUid",
                        column: x => x.UserUid,
                        principalTable: "AppUsers",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RentHistories",
                columns: table => new
                {
                    Uid = table.Column<Guid>(nullable: false),
                    TakenDate = table.Column<DateTime>(nullable: false),
                    ExtendedDueDate = table.Column<DateTime>(nullable: false),
                    ExtendedTimesCount = table.Column<int>(nullable: false),
                    ReturnedDate = table.Column<DateTime>(nullable: true),
                    UserUid = table.Column<Guid>(nullable: false),
                    TakenBookBiaId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentHistories", x => x.Uid);
                    table.ForeignKey(
                        name: "FK_RentHistories_Books_TakenBookBiaId",
                        column: x => x.TakenBookBiaId,
                        principalTable: "Books",
                        principalColumn: "BiaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentHistories_AppUsers_UserUid",
                        column: x => x.UserUid,
                        principalTable: "AppUsers",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestBooks",
                columns: table => new
                {
                    BookId = table.Column<int>(nullable: false),
                    AppUserUid = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestBooks", x => new { x.BookId, x.AppUserUid });
                    table.ForeignKey(
                        name: "FK_RequestBooks_AppUsers_AppUserUid",
                        column: x => x.AppUserUid,
                        principalTable: "AppUsers",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestBooks_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BiaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Uid = table.Column<Guid>(nullable: false),
                    BookId = table.Column<int>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    Rating = table.Column<int>(nullable: false),
                    RatedByUid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Uid);
                    table.ForeignKey(
                        name: "FK_Reviews_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BiaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_AppUsers_RatedByUid",
                        column: x => x.RatedByUid,
                        principalTable: "AppUsers",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UploadHistories",
                columns: table => new
                {
                    AppUserUid = table.Column<Guid>(nullable: false),
                    BookId = table.Column<int>(nullable: false),
                    UploadDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadHistories", x => new { x.AppUserUid, x.BookId });
                    table.ForeignKey(
                        name: "FK_UploadHistories_AppUsers_AppUserUid",
                        column: x => x.AppUserUid,
                        principalTable: "AppUsers",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UploadHistories_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BiaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNotifies",
                columns: table => new
                {
                    BookId = table.Column<int>(nullable: false),
                    Uid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifies", x => new { x.Uid, x.BookId });
                    table.ForeignKey(
                        name: "FK_UserNotifies_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BiaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserNotifies_AppUsers_Uid",
                        column: x => x.Uid,
                        principalTable: "AppUsers",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminNotifies_Uid",
                table: "AdminNotifies",
                column: "Uid");

            migrationBuilder.CreateIndex(
                name: "IX_BookAuthors_AuthorUid",
                table: "BookAuthors",
                column: "AuthorUid");

            migrationBuilder.CreateIndex(
                name: "IX_BookCategories_CategoryId",
                table: "BookCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_TakenByUserUid",
                table: "Books",
                column: "TakenByUserUid");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadHistories_DownloadedBookBiaId",
                table: "DownloadHistories",
                column: "DownloadedBookBiaId");

            migrationBuilder.CreateIndex(
                name: "IX_DownloadHistories_UserUid",
                table: "DownloadHistories",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_RentHistories_TakenBookBiaId",
                table: "RentHistories",
                column: "TakenBookBiaId");

            migrationBuilder.CreateIndex(
                name: "IX_RentHistories_UserUid",
                table: "RentHistories",
                column: "UserUid");

            migrationBuilder.CreateIndex(
                name: "IX_RequestBooks_AppUserUid",
                table: "RequestBooks",
                column: "AppUserUid");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookId",
                table: "Reviews",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_RatedByUid",
                table: "Reviews",
                column: "RatedByUid");

            migrationBuilder.CreateIndex(
                name: "IX_UploadHistories_BookId",
                table: "UploadHistories",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifies_BookId",
                table: "UserNotifies",
                column: "BookId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminNotifies");

            migrationBuilder.DropTable(
                name: "Arrangements");

            migrationBuilder.DropTable(
                name: "BookAuthors");

            migrationBuilder.DropTable(
                name: "BookCategories");

            migrationBuilder.DropTable(
                name: "DownloadHistories");

            migrationBuilder.DropTable(
                name: "IgnoredUsers");

            migrationBuilder.DropTable(
                name: "RentHistories");

            migrationBuilder.DropTable(
                name: "RequestBooks");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "UploadHistories");

            migrationBuilder.DropTable(
                name: "UserNotifies");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "AppUsers");
        }
    }
}
