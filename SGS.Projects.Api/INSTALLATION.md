# Guida all'Installazione e Configurazione

## Prerequisiti

### 1. .NET 8.0 SDK
- Scarica e installa .NET 8.0 SDK da: https://dotnet.microsoft.com/download/dotnet/8.0
- Verifica l'installazione con: `dotnet --version`

### 2. Driver ODBC per SAP HANA
- Scarica il driver ODBC per SAP HANA dal sito SAP
- Installa il driver sul sistema
- Verifica l'installazione nel Gestore DSN ODBC di Windows

### 3. SAP Business One
- SAP Business One installato e configurato
- Service Layer abilitato e accessibile
- UDO Timesheet configurato con i campi richiesti

## Configurazione

### 1. Configurazione ODBC
1. Apri il **Gestore DSN ODBC** di Windows
2. Crea una nuova **System DSN** per SAP HANA
3. Configura i parametri di connessione:
   - **Driver**: SAP HANA
   - **Server**: [indirizzo-server]:30015
   - **Database**: [nome-database]
   - **User ID**: [username]
   - **Password**: [password]

### 2. Configurazione SAP Business One Service Layer
1. Verifica che il Service Layer sia abilitato in SAP B1
2. Annota l'URL del Service Layer (es: https://server:50000/b1s/v2)
3. Verifica le credenziali di accesso

### 3. Configurazione UDO Timesheet
In SAP Business One, crea un UDO chiamato "TIMESHEET" con i seguenti campi:

| Campo | Tipo | Obbligatorio | Descrizione |
|-------|------|--------------|-------------|
| U_Date | Date | Sì | Data del timesheet |
| U_EmployeeId | Text | Sì | ID del dipendente |
| U_ProjectId | Text | Sì | ID del progetto |
| U_ActivityId | Text | Sì | ID dell'attività |
| U_Hours | Numeric | Sì | Ore lavorate |
| U_Description | Text | No | Descrizione del lavoro |
| U_Status | Text | No | Stato del timesheet |

### 4. Configurazione dell'Applicazione
1. Modifica `appsettings.json` o `appsettings.Development.json`
2. Aggiorna la stringa di connessione ODBC
3. Configura i parametri del Service Layer

## Test della Configurazione

### 1. Test ODBC
```bash
# Testa la connessione ODBC
dotnet run --environment Development
```

### 2. Test Service Layer
```bash
# Verifica l'accesso al Service Layer
curl -X POST https://your-server:50000/b1s/v2/Login \
  -H "Content-Type: application/json" \
  -d '{"CompanyDB":"your-db","UserName":"your-user","Password":"your-password"}'
```

### 3. Test API
Una volta avviata l'applicazione:
- Swagger UI: https://localhost:7001/swagger
- Test endpoint: https://localhost:7001/api/timesheet

## Risoluzione Problemi

### Errore: "Driver not found"
- Verifica che il driver ODBC sia installato
- Controlla il nome del driver nella stringa di connessione

### Errore: "Connection failed"
- Verifica l'indirizzo del server SAP HANA
- Controlla le credenziali
- Verifica che la porta 30015 sia aperta

### Errore: "Service Layer not accessible"
- Verifica che il Service Layer sia abilitato
- Controlla l'URL del Service Layer
- Verifica le credenziali di accesso

### Errore: "UDO not found"
- Verifica che l'UDO TIMESHEET sia configurato
- Controlla i nomi dei campi UDO
- Verifica i permessi dell'utente

## Sicurezza

### In Produzione
1. **Gestione Credenziali**: Usa Azure Key Vault o simili
2. **HTTPS**: Configura certificati SSL
3. **Autenticazione**: Implementa autenticazione API
4. **Autorizzazione**: Configura autorizzazioni appropriate
5. **Logging**: Configura logging sicuro

### Best Practices
- Non committare mai credenziali nel codice
- Usa variabili d'ambiente per le configurazioni sensibili
- Implementa rate limiting per le API
- Configura CORS appropriatamente
- Monitora l'accesso alle API

## Supporto

Per problemi tecnici:
1. Controlla i log dell'applicazione
2. Verifica la configurazione ODBC
3. Testa la connessione al Service Layer
4. Contatta il supporto SAP se necessario
