using System.ComponentModel.DataAnnotations;

namespace SGS.Projects.Api.Models
{
    public class Timesheet
    {
        // Campi di sistema SAP B1
        public int? DocEntry { get; set; }
        public string DocNum { get; set; } = string.Empty;
        public string? Code { get; set; } // Campo standard alfanumerico (20)
        
        // Campi di identificazione risorsa
        public string? ResId { get; set; } // Id Risorsa (Alfanumerico 50)
        public string? CardCode { get; set; } // Codice BP (Alfanumerico 100)
        public string? CardName { get; set; } // Ragione Sociale (Alfanumerico 100)
        public string? RefId { get; set; } // Id Referente (Alfanumerico 10)
        public string? RefData { get; set; } // Dati Referente (Alfanumerico 250)
        
        // Campi di progetto e attività
        public string? Project { get; set; } // Progetto (Alfanumerico 100)
        public string? ProjectName { get; set; } // Nome Progetto (Alfanumerico 200)
        public string? SubProject { get; set; } // Sottoprogetto (Alfanumerico 100)
        public string? Activity { get; set; } // Attività (Alfanumerico 100)
        public string? ActivityId { get; set; } // Attività (Alfanumerico 100)
        public string? SubActivity { get; set; } // Attività Sottoprogetto (Alfanumerico 100)
        public string? ActivityName { get; set; } // Nome Attività (Alfanumerico 200)
        
        // Campi temporali principali
        public DateTime Date { get; set; } // Data Attività
        public TimeSpan? TimeStart { get; set; } // Ora Inizio
        public TimeSpan? TimeEnd { get; set; } // Ora Fine
        public TimeSpan? TimePa { get; set; } // Ore Numero Pausa
        public TimeSpan? TimeNF { get; set; } // Ore Numero Non Fatturabili
        
        // Campi ore numeriche
        public decimal? TimeNrPa { get; set; } // Ore Numero Totali
        public decimal? TimeNrNF { get; set; } // Ore Numero Netto
        public decimal? TimeNrTot { get; set; } // Ore Numero Totali
        public decimal? TimeNrNet { get; set; } // Ore Numero Netto
        
        // Campi economici
        public decimal? Price { get; set; } // Prezzo Unitario
        public decimal? LineTotal { get; set; } // Totale Riga
        
        // Campi descrittivi
        public string? DescExt { get; set; } // Descrizione Esterna
        public string? DescInt { get; set; } // Descrizione Interna
        
        // Campi articolo e stato
        public string? ItemCode { get; set; } // Codice Articolo (Alfanumerico 100)
        public string? Status { get; set; } // Stato (Alfanumerico 100)
        public string? Approver { get; set; } // Approvatore (Alfanumerico 100)
        
        // Campi di riferimento fattura
        public string? InvEntry { get; set; } // Id Fattura (Alfanumerico 100)
        
        // Campi destinazione
        public string? DestType { get; set; } // Tipo Destinazione (Alfanumerico 20)
        public string? DestEntry { get; set; } // Id Destinazione (Alfanumerico 10)
        public int? DestLine { get; set; } // Riga Destinazione (Numerico 10)
        
        // Campi base
        public string? BaseType { get; set; } // Tipo Base (Alfanumerico 20)
        public string? BaseEntry { get; set; } // Id Base (Alfanumerico 10)
        public int? BaseLine { get; set; } // Riga Base (Numerico 10)
        
        // Campi originali (per tracking modifiche)
        public string? DescExtOri { get; set; } // Descrizione Esterna Ori
        public TimeSpan? TimeStartOri { get; set; } // Ora Inizio Ori
        public TimeSpan? TimeEndOri { get; set; } // Ora Fine Ori
        public TimeSpan? TimePaOri { get; set; } // Ore Numero Pausa Ori
        public TimeSpan? TimeNFOri { get; set; } // Ore Numero Non Fatturabili Ori
        public decimal? TimeNrPaOri { get; set; } // Ore Numero Totali Ori
        public decimal? TimeNrNFOri { get; set; } // Ore Numero Netto Ori
        public decimal? TimeNrTotOri { get; set; } // Ore Numero Totali Ori
        public decimal? TimeNrNetOri { get; set; } // Ore Numero Netto Ori
        
        // Campi di sistema per compatibilità con modello esistente
        [Required]
        public string EmployeeId { get; set; } = string.Empty; // Mappato su ResId per compatibilità
        
        [Required]
        public string ProjectId { get; set; } = string.Empty; // Mappato su Project per compatibilità
        
        [Required]
        [Range(0, 24)]
        public decimal Hours { get; set; } // Mappato su TimeNrNet per compatibilità
        
        public string? Description { get; set; } // Mappato su DescExt per compatibilità
        
        // Campi di sistema SAP B1
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }

    public class TimesheetCreateRequest
    {
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public string EmployeeId { get; set; } = string.Empty;
        
        [Required]
        public string ProjectId { get; set; } = string.Empty;
        
        [Required]
        public string ActivityId { get; set; } = string.Empty;
        
        [Required]
        [Range(0, 24)]
        public decimal Hours { get; set; }
        
        public string? Description { get; set; }
        
        // Campi aggiuntivi opzionali
        public string? ResId { get; set; }
        public string? CardCode { get; set; }
        public string? CardName { get; set; }
        public string? RefId { get; set; }
        public string? RefData { get; set; }
        public string? Project { get; set; }
        public string? ProjectName { get; set; }
        public string? SubProject { get; set; }
        public string? Activity { get; set; }
        public string? SubActivity { get; set; }
        public string? ActivityName { get; set; }
        public TimeSpan? TimeStart { get; set; }
        public TimeSpan? TimeEnd { get; set; }
        public TimeSpan? TimePa { get; set; }
        public TimeSpan? TimeNF { get; set; }
        public decimal? TimeNrPa { get; set; }
        public decimal? TimeNrNF { get; set; }
        public decimal? TimeNrTot { get; set; }
        public decimal? TimeNrNet { get; set; }
        public decimal? Price { get; set; }
        public decimal? LineTotal { get; set; }
        public string? DescExt { get; set; }
        public string? DescInt { get; set; }
        public string? ItemCode { get; set; }
        public string? Status { get; set; }
        public string? Approver { get; set; }
    }

    public class TimesheetUpdateRequest
    {
        [Required]
        public int DocEntry { get; set; }
        
        public DateTime? Date { get; set; }
        
        public string? EmployeeId { get; set; }
        
        public string? ProjectId { get; set; }
        
        public string? ActivityId { get; set; }
        
        [Range(0, 24)]
        public decimal? Hours { get; set; }
        
        public string? Description { get; set; }
        
        // Campi aggiuntivi opzionali
        public string? ResId { get; set; }
        public string? CardCode { get; set; }
        public string? CardName { get; set; }
        public string? RefId { get; set; }
        public string? RefData { get; set; }
        public string? Project { get; set; }
        public string? ProjectName { get; set; }
        public string? SubProject { get; set; }
        public string? Activity { get; set; }
        public string? SubActivity { get; set; }
        public string? ActivityName { get; set; }
        public TimeSpan? TimeStart { get; set; }
        public TimeSpan? TimeEnd { get; set; }
        public TimeSpan? TimePa { get; set; }
        public TimeSpan? TimeNF { get; set; }
        public decimal? TimeNrPa { get; set; }
        public decimal? TimeNrNF { get; set; }
        public decimal? TimeNrTot { get; set; }
        public decimal? TimeNrNet { get; set; }
        public decimal? Price { get; set; }
        public decimal? LineTotal { get; set; }
        public string? DescExt { get; set; }
        public string? DescInt { get; set; }
        public string? ItemCode { get; set; }
        public string? Status { get; set; }
        public string? Approver { get; set; }
    }
}
