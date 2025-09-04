# Besöks-Registreringssystem -- Utvecklingsplan

### Jag behöver:

- [x] Frontend - Static Web App HTML/JS --> GitHub Pages
- [x] Backend - Azure Functions som kommunicerar med databasen
- [x] Databas - Mitt val: Azure SQL Database
- [x] Loggning - ILogger: Övervakning via Azures Application Insights

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
- [x] Lägg till nödvändig loggning

#### Frontend (HTML/CSS/JS)

- [x] Skapa formulär som låter en användare registrera ett besök
- [x] Koppla funktionalitet till registreringen av besök med en fetch() som kontaktar API:t / Azure Functions URL
- [x] Ge output tillbaka till användaren för att bekräfta besöket

#### Deploya / Integrera

- [x] Skapa Azure Functions på Azure och deploya.
- [x] Skapa Static Web App på Azure och deploya (GitHub-pages)
- [ ] Testa och utforska Application Insights loggning och eventuella inställningar(?)
- [ ] Testa det fullständiga systemet - lokalt och public

#### Extra

- [x] Lösa hur jag hanterar access till min SQL Databas med en dynamisk IP - Kör på Firewall rules.
- [ ] Komma på hur jag kan tillåta CORS mellan min Function App och github pages och inte använda asterisk
- [x] Hantera tomt input fält som ger "." värde - ska ej gå att inmata
- [x] Frontend: Hantera input firstName från request är "" - la till required som HTML-attribut
- [x] Backend: Checka så att request body properties inte är whitespace eller null
- [ ] Loggas databasens händelser via Application Insights elelr snappar AI endast upp från kodbasen?
- [x] Byt från Secrets/SQL Authentication till Managed Identity /+ Entra ID?
- [x] Byta namn från 'Timestamp' (property) till nåt annat? --> CheckInTime
- [x] Byta från SQL Authentication/Secrets till Managed Identity för Azure Functions/SQL-Server
- [x] Kolla om jag ens behöver HttpRequest req i function-parametern nu när jag kör "[FromBody]"- inte i mitt fall. Kan vara bra om man vill ha information ut från den (objektet har väldigt mycket) men i mitt fall behöver jag inte det.
- [ ]

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
- Stötte på CORS-Origin restriction vid lokal testning. Eftersom addressen och porten skiljer sig mellan min API (azure function) och html (eg live server eller fileURL), så stöter jag på CORS restriction.
- Löste det genom att gå in i local.settings.json och lägga till "Host" "CORS": "\*" för att tillåta alla. Tror det alternativet är helt okej när det handlar om local development. Men man bör nog vara mer försiktig i produktion.
- La till ett responseMessage när besökaren registrear, beroende på success eller failure.

#### Datum

- Försöker komma ihåg hur jag converterar UTCdatumet i JavaScript men lyckas inte få det som jag vill..återkommer..
- Testade toLocaleString, date.parse utan succé men det verkar som JavaScript inte förstår att det var utc och därmed testade jag lägga till ett 'z' som suffix som ska representera detta och då fungerade det.
- Jag upptäckte att det hade troligen varit smartare om man använt DateTimeOffset som datatyp istället för DateTime så att man slapp konkatenera och lägga till ett "Z" i frontend. Funderar på att fixa det men det hade inneburit: schema change i databasen och att uppdatera VisitorModel.
- Använde .ToLocaleString med optional parameters för att justera displayen av strängen för användaren.

#### Deployment Function App

- VSC Extension > Workspace > Function App > Deploy to Azure > Overwrite > RegisterVisitor dök upp i portalen. Success.
- Det som gör att min Function App upptäcker min function är [Function("RegisterVisitor")] i koden, om jag hade flera av dessa hade den förmodligen registrerat flera functions i min Function App.
- Eftersom jag använder Environment.GetEnvironmentVariable så bör jag nog använda mig av 'App Settings' i portalen och inte 'Connection String'.
- Nu är min Function App deployed och Status = Running. Jag har nu en Default Domain: func-uppgift1.azurewebsites.net att testa mitt api med.
- Testade endpointen med min public domän och först gavs det ett 500 error men jag gissar att detta är för att Function Appen eller databasen kanske var idle och det är väl egentligen detta som är en del av grejen med 'serverless'. Andra försöket fungerade och det gav 200 response.
- När jag skulle logga in på min SQL Database tilläts inte min IP så jag gissar att den är dynamisk och skiftar med den internetleverantör jag har. Så behöver nog hitta en lösning på detta.

- Deployade till GitHub Pages och stötte på error: `Blocked loading mixed active content “http://func-uppgift1.azurewebsites.net/api/RegisterVisitor”` vilket jag löste genom att ändra min Function App URL från http till https.
- Nästa steg är att se om jag kan fixa CORS - i min Function App hittade jag "CORS" under API-fliken och la till min origin: https://www.ludwigstenberg.github.io
- Lyckades inte få det att fungera med CORS med min specifika origin. Istället skickade jag in '\*' som en tillfällig lösning för att tillåta ALLT.

- La till en del basic css för sidan och det verkar som att GitHub Pages märker av och uppdaterar om man gör en push till sitt repository. Neat.

- Funderar över felhantering: Vilka failures skulle kunna ske i min function?

1. Felaktig request body / firstName
2. Databas / connection error

- Märkte att jag kan ta bort `if newVisitor == null och dess return status` eftersom databasen hade throwat SqlException iom constraints.

- Har issues när jag försöker skicka en valid request och får 500 tillbaka. Lägger till loggning och ser om jag kan hitta var problemet uppstår.

- La till loggning och felhantering för om en connection string är invalid eller inte hittas. Detta är dock förmodligen inte det som skapar problemet.

- Hittade mitt problem. Min loggning kom fram till att anslutningen försökte öppnas men den gav aldrig ett success-meddelande. Vilket fick mig att inse att min IP inte var tillåten. Att lägga till den fixade problemet. Behöver en lösning på detta.

- La till loggning för nästan varje steg i min azure function. > Deployade för att testa i cloudet.

- Blev sugen på att lägga till LastName och EmailAddress också vilket kommer innebära:
- [x] Ändra i SQL Database schema (ALTER TABLE, ADD..)
- [x] Lägga till i ModelVisitor
- [x] Uppdatera API
- [x] Lägga till ytterligare ett input field i HTML
- [x] Lägga till funktionalitet via JavaScript

- Blev sugen på att sätta en timer funktionalitet på mitt responseMessage när en användare registrerar. Jag tänker pga nån slags privacy. https://developer.mozilla.org/en-US/docs/Web/API/Window/setTimeout
- använde mig av inline-css på elementet: .style.display = "none";

#### JSON Parse --> Model Binding

- Jag tycker inte om hur jag behöver parsa/deserialsera med JSON-metoder och är nyfiken om det går att använda ModelBinding i en Azure Function som det exempelvis går i ett MVC-pattern i en Controller.
- Läsning: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cfunctionsv2&pivots=programming-language-csharp#payload
- https://stackoverflow.com/questions/44660314/how-can-i-do-modelbinding-with-httptrigger-in-azure-functions
- Det ser ut att fungera så jag la till min VisitorModel some en parameter för att testa. Först gick det inte men jag insåg att jag missat använda rätt using som länken visar.
- Så det fungerar, och jag behöver därmed inte använda mig av någon JSON-converter.
- Jag har kommit fram till att jag nog vill lägga till en VisitorInputModel så att jag inte exponerar min domän-modell som en parameter. Den innehåller dessutom 'Id' och 'Timestamp' som jag inte tar emot via request. Därmed la jag enbart till 'FirstName', 'LastName' och 'EmailAddress'i min VisitorInputModel.

#### Model Validation

- Jag vill validera så att min VisitorInputModel och i vanliga fall är jag inte särskilt förtjust i att använda validation attributes pga att det gör att klassen blir rörig. Men eftersom jag använder mig av en InputModel och inte min domän-modell tror jag det blir okej (förutsatt att det faktiskt går att använda validation attributes inom detta system, vilket det bör)
- Kom fram till att det INTE går att använda sig av Data Annotations med Validation Attributes inom en Azure Function och man behöver alltså skapa sin egen validation logic. Så det är vad jag tänker göra, och då kanske VisitorInputModel blir lite onödig i mitt fall. Så jag tar bort den igen.
- Kommit fram till att jag ska göra min egna helper för att validera modellen och då behöver jag:
  IsNullOrWhitespace (hanterar required och saknad av värde)
  Max/Min-length (hanterar längd-constraints)
  Email-validation (Kan använda mig av Microsofts inbyggda)

- Hittade en ännu bättre lösning som jag körde på:
  https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.validator?view=net-9.0
- Att använda mig av Validator-klassen för att validera Data Annotations! Vilket innebär att jag faktiskt kan köra på validation attributes i modellen, nice.
- Validator.TryValidateObject tar in parameters: object, context, validationResults, boolean:
  object: är det objektet med data som vi vill validera
  context: är själva validerings-informationen (dvs attributerna) för vårt objekt vilket finns i modellen
  validationResults är en lista där våra resultat lagras som errors
  boolean: true/false har att göra med om vi vill validera alla properties eller om vi vill sluta om vi når first failure.
- La till default assignment av = DateTime.UtcNow för Timestamp i modellen
- Detta innebär att den initialiseras vid model binding och jag får en mer exakt tid än om datumet skulle skapats i databasen by default.
- La till 'Timestamp' i INSERT-operationen också.
- Tog bort JsonException nu när jag inte använder mig av JsonParse

#### HTML/CSS Fix & Trix

- Fick sug på att lägga ner lite tid på CSS så gav hela sidan en liten makeover.
- Testade lära mig lite om hur ::before fungerade och det är ett smidigt sätt att inte behöva skapa ett element för att lägga till lite extra design.
- Upptäckte även ett problem där det andra meddelandet inte dök upp efter att det första försvinner efter setTimeout. Jag kom på att jag behöver ju återställa värdet på mitt responseMessage varje gång a la ""; så det gjorde jag, plus att jag satte tillbaka style.display från "none" till "block".

#### Byta från SQL Authentication --> Managed Identity

- Function App > System Assigned > ON
- SQL-server > Microsoft Entra ID > Set Admin > mig själv
- Lägga till en user för function app (external provider) i databasen med permissions. db_datareader och db_datawriter.
- Eftersom jag nu kör Managed Identity använder jag en annan connection string utan user och password. Eftersom vi har satt admin till oss själva förstår azure detta, men när vi kör lokalt behöver vi på nåt sätt autentisera vilket vi gör genom att vara inloggade på azure från vår lokala miljö eg VSC / CLI.
- Eftersom mitt azure-konto har admin-roll på SQL-servern så har den full permission att utföra databas-operationer.
- Uppdaterat SqlConnectionString i både local.settings.json och Env variables i Function App: Authentication=Active Directory Default.
- Att skifta från SQL Authentication och att gå på min Identity istället innebär att jag inte behöver sätta upp nya firewalls så fort min IP-address ändras och jag inte kommer åt servern.
- Man kan ha kvar SQL Authentication om man vill men jag tar bort det:
  Enabled Microsoft Entra-only authentication för servern i settings > Entra ID.
  Att ta bort SQL Authentication innebär ju att jag reducerar min attack-yta och jag kommer ändå inte använda SQL Auth så det är en onödig "risk".

#### Byta från 'Timestamp' till 'CheckInTime'

- Ändrade VisitorModel property 'Timestamp' till 'CheckInTime'
- Ersatte alla 'Timestamp' i RegisterVisitor (function) med 'CheckInTime'
- Ändrade från newVisitor till visitorResponse för jag tycker det är tydligare
- Döpte om min column med: EXEC sp_rename 'Visitors.Timestamp', 'CheckInTime', 'COLUMN';

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

CORS
https://stackoverflow.com/questions/43767255/cors-with-azure-function-from-localhost-not-cli

CORS
https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-csharp
