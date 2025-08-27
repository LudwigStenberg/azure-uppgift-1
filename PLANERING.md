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

- [ ] Skapa en Azure Function via extension i VSC
- [ ] Skriv en HTTP-trigger-funktion
- [ ] (Lokalt) Testa min funktion/endpoint via testing-software som Bruno (Behöver inte vara kopplad till databas ännu).

#### Integration av SQL Database

- [ ] Skapa min Azure SQL Database
- [ ] Hämta min connection sträng från min SQL Database i portalen
- [ ] Skapa min entity/model för besökare (eg. VisitorModel: Id, Name, Timestamp)
- [ ] Testa databasanslutning lokalt (via local.settings.json?)
- [ ] Spara requests/besökare till databasen
- [ ] Returnera nödvändig info till klienten

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
