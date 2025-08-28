# Besöks-Registreringssystem -- Utvecklingsplan

### Jag behöver:

- [ ] Frontend - Static Web App HTML/JS
- [ ] Backend - Azure Functions som kommunicerar med databasen
- [ ] Databas - Mitt val: Azure SQL Database
- [ ] Loggning - ILogger: Övervakning via Azures Application Insights

### Plan för utförande:

Mitt fokus ligger till en början på local development och få den biten att fungera. När jag fått det och allt ser bra ut, vill jag deploya och övergå till molnet. Jag vill testa varje komponent enskilt innan jag integrerar nya.

#### Resource Group

- [x] Skapa rg-uppgit1

#### Azure Function

- [x] Skapa en Azure Function via extension i VSC
- [x] Skriv en HTTP-trigger-funktion
- [x] (Lokalt) Testa min funktion/endpoint via testing-software som Bruno (Behöver inte vara kopplad till databas ännu).

#### Integration av SQL Database

- [x] Skapa min Azure SQL Database
- [x] Hämta min connection sträng från min SQL Database i portalen
- [x] Skapa min entity/model för besökare (eg. VisitorModel: Id, Name, Timestamp)
- [x] Testa databasanslutning lokalt (via local.settings.json?)
- [x] Spara requests/besökare till databasen
- [x] Returnera nödvändig info till klienten
- [ ] Lägg till nödvändig loggning

#### Frontend (HTML/CSS/JS)

- [ ] Skapa formulär som låter en användare registrera ett besök
- [ ] Koppla funktionalitet till registreringen av besök med en fetch() som kontaktar API:t / Azure Functions URL
- [ ] Ge output tillbaka till användaren för att bekräfta besöket

#### Deploya / Integrera

- [ ] Skapa Azure Functions på Azure och deploya.
- [ ] Skapa Static Web App på Azure och deploya.
- [ ] Testa och utforska Application Insights loggning och eventuella inställningar(?)
- [ ] Testa det fullständiga systemet

---

## Anteckningar och reflektioner

#### Setup Function App via VSC Extension 'Azure Functions'

- Function App Name: func-uppgift1 ()
- .NET 8 eftersom den är stabil och är LTS (jämfört med .NET9 som inte är LTS)
- Resource Authentication: Väljer 'Secrets' för att lära mig det men förstår att 'Managed Identity' verkar vara ett mer säkert val.

Denna extension skapade:
App Service Plan, Function App, Application Insights, Storage Account och Log Analytics Workspace automatiskt, inklusive en egen resource group som dessa grupperades i.

#### Skapade func-project via command palette

- HTTP-trigger namn: "RegisterVisitor"
- Namespace: "VisitorRegistration"
- Access rights: Anonymous
  - Anonymous: Vem som helst kan anropa min funktion med min URL. Fungerar som en public API endpoint.
    Exempel: https://yourfunction.azurewebsites.net/api/RegisterVisitor
  - Function: Kräver en key där varje function har sin egen. Säkrare än anonymous, naturally..
    Exempel: https://yourfunction.azurewebsites.net/api/RegisterVisitor?code=abc123xyz
  - Admin: Kräver en 'Master Key' som kan komma åt vilken function som helst i en Function App. Verkar mest användas för administrativa operationer.
  - Mitt val: Anonymous - Varför? Tanken är att det är en public tjänst och jag vill att besökare ska kunna registrera sig i mitt system utan att behöva en nyckel.

#### Skapa koden för min HTTP-trigger function

- Testat den ursprungliga koden med "get" för att se om det finns aktivitet när man kör dotnet run och skickar in ett namn som query string. Fungerar.

- HTTP-TRIGGER REQUEST
- 'POST' REQUEST
- TA EMOT REQUEST I FORM AV EN JSON BODY
- LÄS IN TILL EN STRÄNG 'requestBody'
- DESERIALIZE TILL OBJEKT (VisitorModel)
- ÖPPNA EN ANSLUTNING TILL DATABAS
- SPARA DATA TILL DATABASEN MED QUERY
- STÄNG ANSLUTNING TILL DATABAS (Using)
- IF'SUCCESS' - RETURNERA DET SKAPADE OBJEKTET TILL FRONTEND
- IF 'FAILURE' - RETURNERA 400BR ELLER LIKNANDE

Fick inte .Deserialize att fungera som jag ville och eftersom jag bara hade en property (firstName) i requesten så valde jag att testa JsonNode.Parse för att extrahera värdet av 'firstName'.

- Har nu testat köra en POST request via Bruno lokalt med succé.

- Skapat en SQL Databas på portalen och om jag förstår det rätt så kan jag lägga till dess ConnectionString i local.settings.json.
- Valde LRS istället för GRS (billigare och jag behöver inte så avancerad backup)

- Det verkar som jag behövde skapa en Server (logisk) för att hantera min SQL Database.

- Valde SQL Authentication som autentiseringsmetod (går att ändra sen)

- Jag var inte tillåten att logga in på min databas med min IP-address så jag la till den via Server > Networking > Public Access

- Vägde alternativen mellan att skapa min databas-tabell "Visitors" i kod eller direkt i portalen. Jag gjorde det i portalen. Alltså i databasens 'Query Editor' och fick då testa på T-SQL vilket var ganska likt PSQL med några detaljskillnader.

- Märkbara skillnader var T-SQLs IDENTITY(1,1) jämfört med PSQLs SERIAL för auto-incrementing och DATETIME2 + GETUTCDATE() för T-SQL.

- Lade till nytt NuGet package: "dotnet add package Microsoft.Data.SqlClient"

- Satte upp en SqlConnection och hämtade SqlConnectionString i local.settings.json vilket verkar hanteras som environment variables när det körs och därför enklare istället för att jag eg injectar IConfiguration och hämtar med GetConnectionString.

- Just nu hämtar jag connection string direkt utan att checka för null. Nu vet jag att den kommer att vara populerad men jag bör nog lägga till en null-check i ett senare stadie.

- Märkte att det var onödigt att skapa ett newVisitor objekt när jag ändå enbart kommer att skicka in FirstName via SQL-query. Tog bort. Jag lär däremot skapa ett när jag hämtar data från databasen för att returnera objektet till frontend.

- Eftersom jag använder raw SQL och inte EF så får jag inte tillbaka min resurs från databasen när jag skapar en ny entitet. Jag kom fram till att jag ska använda en "Output clause" i min query så att jag hämtar datan för den resurs som skapas på samma gång, atomiskt. Vilket är säkrare än om jag hade använt mig av t.ex två stycken queries med en INSERT och en SELECT då det skulle kunna uppstå issues med concurrency/race-conditions då flera användare ex registrerar samtidigt.

- Eftersom vi förväntar oss att få tillbaka data via vår query kommer jag därför att använda ExecuteReader() och inte ExecuteNonQuery().

#### SQL Database operation

- Hämtar och skapar connection - sträng hämtad från local.settings.json via GetEnvironmentVariable

- Skapat min query string för mitt kommando

- La till en OUTPUT clause för att i samma operation hämta värdena för mitt nyskapade Visitor-objekt.

- Öppnade anslutningen (eller, hämtade från .NETs connection pool)

- Skapat mitt command som fått info om: query-strängen och min connection

- Hanterar SQL Injection genom command.Parameters så att det inte finns manipulerbara variabler i
  query-strängen.

- Eftersom vi får tillbaka mer än ett värde och i detta fall värdet från 3 kolumner kommer jag att använda mig av ExecuteReaderAsync().

- GetInt32, GetString osv tar inte emot namnen på kolumner och behöver istället index-värdet som representerar det. Därmed använder jag mig av GetOrdinal-metoden för att hämta ut dessa index för att sen kunna använad dem i konstruktionen av newVisitor.

- Jag valde att kalla på GetOrdinal direkt i property assignment eg: `Id = reader.GetInt32(reader.GetOrdinal("Id"))` men om det hade varit flera entiteter och rader som hämtades så hade det inte varit optimalt eftersom reader.GetOrdinal hade behövts utföras inför varje look-up. Alternativet är att man hade kunnat spara och cacha dem ovanför i variabler. Men eftersom jag bara vill en rad och tre kolumner använder väljer jag det mer smidiga och "rena" alternativet.

- Testade anslutning mellan servern och min SQL connection. Failure pga obehörig IP-address. Gick in på Azure Portal > Networking > Add Firewall rule och lade till min test-IP. Fungerade.

- Skickade request med Bruno > Gick in i Query Editor och läste från Visitors - Ny data inlagd. Succé!

#### Påbörjade frontend

- Skapade index.html, app.js
- Skapade en form med submit button
- La till event-listener för submit + preventDefault
- Skapade en POST fetch metod för att skicka 'firstName' till min azure function.
- Stötte på CORS-Origin restriction.

### Mina resurser:

HTTP-Trigger
https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cfunctionsv2&pivots=programming-language-csharp

StreamReader
https://learn.microsoft.com/en-us/dotnet/api/system.io.streamreader.readtoendasync?view=net-9.0

JsonNode
https://learn.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonnode?view=net-9.0

T-SQL
https://learn.microsoft.com/en-us/sql/t-sql/language-reference?view=sql-server-ver17#t-sql-compliance-with-the-sql-standard

Manage Function App (CORS)
https://learn.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings?tabs=azure-portal%2Cto-premium#cors
