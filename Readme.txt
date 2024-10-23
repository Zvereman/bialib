Migrations instructions

dotnet ef migrations add MIGRATION_NAME -s Bia.Internal.BookLibrary.Data

# where the MIGRATION_NAME is your migration name (thanks, captain obvious)
# After that you can edit created migrations and execute this apply migrations to database:

Update-Database

# To remove migrations execute:

dotnet ef migrations remove -s Bia.Internal.BookLibrary.Data