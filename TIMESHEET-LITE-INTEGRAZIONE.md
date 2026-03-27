# Integrazione endpoint `POST /api/Timesheet/lite`

Questa guida descrive in modo operativo come un software esterno deve chiamare l'endpoint "lite" per creare un timesheet.

## Endpoint

- **Metodo**: `POST`
- **Path**: `/api/Timesheet/lite`
- **Content-Type**: `application/json`

URL completo (esempio):

`https://<host-api>/api/Timesheet/lite`

---

## Modello di input (body JSON)

Il body deve rispettare il modello `TimesheetCreateRequestLite`.

```json
{
  "date": "2026-03-26T00:00:00",
  "resId": "EMP001",
  "project": "PRJ-0001",
  "activityId": "ACT-010",
  "hours": 8.0,
  "desc": "Analisi requisiti e allineamento tecnico"
}
```

### Campi richiesti

- `date` (`DateTime`, obbligatorio): data attività.
- `resId` (`string`, obbligatorio): identificativo risorsa/dipendente.
- `project` (`string`, obbligatorio): codice progetto.
- `activityId` (`string`, obbligatorio): codice attività del progetto.
- `hours` (`decimal`, obbligatorio): ore da registrare, **deve essere > 0**.
- `desc` (`string`, obbligatorio): descrizione attività.

> Nota: oltre al vincolo di obbligatorietà, il controller applica una validazione esplicita su `hours`: se `null` o `<= 0` la chiamata viene rifiutata.

---

## Comportamento server (risoluzione automatica dipendenze)

Con il payload "lite" non devi inviare i dati anagrafici completi del timesheet.  
Il server li risolve automaticamente:

1. Cerca il progetto tramite `project`.
2. Recupera le attività del progetto.
3. Cerca l'attività con codice uguale a `activityId` (confronto case-insensitive).
4. Verifica che il progetto abbia dati anagrafici minimi completi (`CardCode` e `Name`).
5. Costruisce e crea il timesheet finale su SAP B1 Service Layer.

---

## Risposte dell'endpoint

## `201 Created` (successo)

Restituisce un oggetto `Timesheet` completo (JSON) e imposta anche:

- Header `Location` verso `GET /api/Timesheet/{docEntry}`

Struttura del payload di risposta (`Timesheet`):

- `docEntry` (`int?`)
- `code` (`string?`)
- `resId` (`string?`)
- `cardCode` (`string?`)
- `cardName` (`string?`)
- `refId` (`string?`)
- `refData` (`string?`)
- `project` (`string?`)
- `projectName` (`string?`)
- `subProject` (`string?`)
- `activity` (`string?`)
- `activityId` (`string?`)
- `subActivity` (`string?`)
- `activityName` (`string?`)
- `date` (`DateTime`)
- `timeStart` (`int?`)
- `timeEnd` (`int?`)
- `timePa` (`int?`)
- `timeNF` (`int?`)
- `timeNrPa` (`decimal?`)
- `timeNrNF` (`decimal?`)
- `timeNrTot` (`decimal?`)
- `timeNrNet` (`decimal?`)
- `descExt` (`string?`)
- `descInt` (`string?`)
- `status` (`string?`)

Esempio di risposta:

```json
{
  "docEntry": 12345,
  "code": "TS-2026-000123",
  "resId": "EMP001",
  "cardCode": "C0001",
  "cardName": "Cliente Demo",
  "refId": null,
  "refData": null,
  "project": "PRJ-0001",
  "projectName": "Progetto Demo",
  "subProject": null,
  "activity": "ACT-010",
  "activityId": "ACT-010",
  "subActivity": null,
  "activityName": "Analisi",
  "date": "2026-03-26T00:00:00",
  "timeStart": null,
  "timeEnd": null,
  "timePa": null,
  "timeNF": null,
  "timeNrPa": null,
  "timeNrNF": null,
  "timeNrTot": 8.0,
  "timeNrNet": 8.0,
  "descExt": "Analisi requisiti e allineamento tecnico",
  "descInt": null,
  "status": "Aperto"
}
```

## `400 Bad Request`

Casi principali:

- payload non valido rispetto al modello (`ModelState` non valido);
- `hours` assente o minore/uguale a zero:
  - testo risposta: `"Il campo Hours deve essere maggiore di zero"`;
- progetto trovato ma dati anagrafici incompleti:
  - testo risposta: `"Dati anagrafici incompleti per il progetto {project}"`.

## `404 Not Found`

- Progetto non trovato:
  - `"Progetto {project} non trovato"`
- Attività non trovata per il progetto:
  - `"Attività {activityId} non trovata per il progetto {project}"`

## `500 Internal Server Error`

- errore interno non gestito durante il flusso di creazione:
  - `"Errore interno del server durante la creazione del timesheet lite"`

---

## Esempio completo chiamata HTTP

```bash
curl -X POST "https://<host-api>/api/Timesheet/lite" \
  -H "Content-Type: application/json" \
  -d "{
    \"date\": \"2026-03-26T00:00:00\",
    \"resId\": \"EMP001\",
    \"project\": \"PRJ-0001\",
    \"activityId\": \"ACT-010\",
    \"hours\": 8.0,
    \"desc\": \"Analisi requisiti e allineamento tecnico\"
  }"
```

---

## Note operative per software esterno

- Inviare sempre `date` in formato ISO 8601 (`yyyy-MM-ddTHH:mm:ss`).
- Considerare `activityId` come codice attività nel contesto del progetto.
- Gestire esplicitamente gli errori `400` e `404` mostrando il messaggio testuale ricevuto.
- Dopo un `201`, usare `docEntry` restituito per eventuali operazioni successive (`GET`, `PUT`).
