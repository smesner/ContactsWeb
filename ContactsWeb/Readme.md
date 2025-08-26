ContactsWeb je ASP.NET Core MVC aplikacija s SQL Server bazom podataka za upravljanje kontaktima.
Aplikacija nudi:

- Unos kontakata (moguće je unijeti duplikate prema zahtjevu zadatka, uz vremensko ograničenje od 1 minute).
- Dopunu podataka kontakta pozivom na vanjski API: https://jsonplaceholder.typicode.com/users.
- Prikaz svih unešenih kontakata s ".biz" domenom putem SQL view-a.
- Slanje email obavijesti o novim kontaktima.
- Strukturirano logiranje pomoću **Serilog**, s podrškom za konzolu i datoteke.

## Konfiguracija

Sva konfiguracija nalazi se u `appsettings.json`.

Potrebno je podesiti:

1. **Connection string** za spajanje na SQL Server.
2. **SMTP postavke** za slanje emaila.
3. (Opcionalno) **Putanju za logove** – po defaultu logovi se pohranjuju u `logs` direktorij.

Napomena: SQL tablica i view za `.biz` kontakte automatski se kreiraju pri pokretanju aplikacije.
