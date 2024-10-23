# BookLibrary
----

Бакэнд + фронт (front: jQuery+BootStrap, back: .NET CORE) для задачи [INFSYS-1871](https://jira.dellin.ru/browse/INFSYS-1871). Сайт для отдела персонала по учету книг. 

Сайт: http://apps.bia-tech.ru/books (сервер m1-bots)

- сторонний проект по вырезанию логотипов из pdf файлов - https://gitlab.dellin.ru/CSProjects/pdflogoremover
- консоль для планнового оповещения - \\\\m1-bots\Bia.Internal.BooksLibrary.Notification (смотрит в базе пользователей у которых просрочены книги и оповещает их по емейлу) в планировщике задач так и называется Bia.Internal.BooksLibrary.Notification

дефолтные настройки (емей, база, учетка на сервере для авторизироного доступа)
```
 "MailServer": "mail.dellin.local",
  "MailEmail": "biahelper@bia-tech.ru",
  "MailUser": "mbx-biahelper",
  "MailPassword": "%f1dE)tSd7J(5h%7",

  "ConnectionStrings": {
    "dbConnection": "Server=localhost;Port=3306;Database=booklib_dev;Uid=root;Pwd=aZx11264"
  },
  
msp_auth_username = svc_SPBTChatBot  //логин учетки svc_SPBTChatBot на m1-bost
msp_auth_password = lsfFjhA5p0P5sM3Z //пароль учетки svc_SPBTChatBot на m1-bost
```

|итерация|задача|описание|
|--|------|----|
|1|https://jira.dellin.ru/browse/INFSYS-1871|основные страницы пользователь и админка|
|2|https://jira.dellin.ru/browse/INFSYS-2628|доработка админки + емейл рассылка|
|3|https://jira.dellin.ru/browse/INFSYS-2726|поддержка электронных книг + пользовательское соглашение + оптимизация для мобильных девайсов |

----

# Структура:

 - Bia.Internal.BookLibrary.Notification - консоль для оповещения на почту о задолжности книг (работает на m1-bots, pfgecrftncz d task sheduler) + шаблоны писем
 - Bia.Internal.BookLibrary - сайт
 - Bia.Internal.BookLibrary.Data - бд
 - EmailSender - либа для рассылки емейлов + шаблоны для писем
 - BooksLoader - создает в базе записи книг на основе excel файла

# Todo 

- плохо реализован метод создания книги 
- может глючить таблица при малой ширине
- много лишнего js и css
- апишка для закачивания/скачивания книг открыта т.е. любой юзер может скачать/закачать люббой файл на сервер


----
**Hell Yeah!**
