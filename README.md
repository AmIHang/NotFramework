# NotFramework

Framework für die modulare Entwicklung von SPA-Modulen und -Applikationen.

Die Kernidee: UI-Struktur (Forms, Layouts, etc.) wird **server-seitig deklariert** und als JSON-Modell an den Client gesendet. Der Client rendert daraus die Oberfläche — ohne dass Frontend-Code für jede neue UI geschrieben werden muss.

---

## Vision

```
┌─────────────────────────────────────────────────────┐
│                    Developer                        │
│  definiert Modell + UI-Deklaration (server-seitig)  │
└──────────────────────┬──────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────┐
│                  .NET Backend                       │
│  - Business-Objekte (ORM via EF Core)               │
│  - UI-Modell generieren & als JSON serialisieren    │
│  - REST API                                         │
└──────────────────────┬──────────────────────────────┘
                       │ JSON (UI-Modell + Daten)
                       ▼
┌─────────────────────────────────────────────────────┐
│                React Frontend                       │
│  - empfängt JSON-Modell                             │
│  - rendert UI dynamisch daraus                      │
│  - keine manuellen UI-Komponenten pro Feature       │
└─────────────────────────────────────────────────────┘
```

---

## Technologie-Stack

| Schicht | Technologie |
|--------|-------------|
| Backend | .NET 8 |
| Datenpersistenz | Entity Framework Core |
| Datenbank | PostgreSQL (via Npgsql) |
| Frontend | React |
| Kommunikation | REST / JSON |

---

## Architektur

### Backend (`/Core`)

```
NotFramework.sln
└── Core/
    ├── NotModel/    – Business-Objekte, Metamodell (ClassInfo, PropertyInfo)
    ├── NotCore/     – Kern-Abstraktionen (ISession, ISessionFactory, IEntityBroker)
    ├── NotEFCore/   – EF Core Implementierung (Session, ModelDefinition, Mapping-Strategien)
    └── NotPgSql/    – PostgreSQL-Provider
```

**Kernprinzipien:**

- Alle Domain-Objekte erben von `BusinessObject` (mit `OID` als Primary Key)
- `ClassInfo` / `PropertyInfo` beschreiben das Metamodell typsicher — Basis für spätere visuelle Modellierung
- `ISession` / `IEntityBroker` kapseln alle Persistenzoperationen
- Inheritance-Mapping pluggbar: **TPH** (Table-per-Hierarchy) und **TPT** (Table-per-Type)
- Modelle können via `ModelDefinition.Include()` modular zusammengesetzt werden

**Geplant:**
- Das Datenmodell soll in einer späteren Ausbaustufe **visuell erstellt** werden können (Low-Code-Designer)

### Frontend

- React SPA
- Empfängt ein JSON-Modell vom Server
- Rendert UI-Elemente (Forms, Listen, etc.) **deklarativ** basierend auf dem Modell
- Kein manuelles Schreiben von UI-Code pro Feature nötig

---

## Aktueller Stand

> Stand: 2026-03-28 — früher Prototyp

### Implementiert

- [x] `BusinessObject` Basisklasse mit `OID`
- [x] `ClassInfo` / `PropertyInfo` Metamodell-Infrastruktur
- [x] Property-Typen: `String`, `Int`, `Bool`
- [x] `ISession`, `ISessionFactory`, `IEntityBroker` (Abstraktion)
- [x] `Session`, `SessionFactory`, `EntityBroker` (EF Core Implementierung)
- [x] `ModelDefinition` mit Modell-Komposition (`Include()`)
- [x] TPH- und TPT-Inheritance-Mapping-Strategien
- [x] PostgreSQL-Provider
- [x] `IWebNativeSupport` / `JsonWriter` (Stub für JSON-Serialisierung)
- [x] `Culture` / Lokalisierung (Grundstruktur)

### Noch offen / geplant

- [ ] REST API Layer (.NET — Endpunkte für UI-Modell + Daten)
- [ ] JSON UI-Modell-Definition (Schema für Forms, Listen, Navigation etc.)
- [ ] React Frontend (dynamisches Rendering aus JSON-Modell)
- [ ] Weitre PropertyInfo-Typen (DateTime, Decimal, Enum, Navigation/Reference)
- [ ] Query-Builder / LINQ-Integration über `ISession.Query<T>()`
- [ ] Visueller Modell-Designer (Low-Code, spätere Stufe)
- [ ] Unit Tests
- [ ] SQLite-Provider (für Tests)
- [ ] NuGet-Packaging
