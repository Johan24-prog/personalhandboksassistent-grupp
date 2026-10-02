# Personalhandboksassistent

## Konfigurera .env

Skapa en `.env`-fil i projektroten. Appen läser in filen vid start. Ersätt värdena inom `<...>` med dina Azure-värden. `.env` är ignorerad av Git och ska inte checkas in.

```dotenv
# Krävs för att chatten ska kunna anropa Azure OpenAI
AZURE_OPENAI_ENDPOINT=https://<resurs>.openai.azure.com
AZURE_OPENAI_API_KEY=<api-nyckel>
AZURE_OPENAI_CHAT_DEPLOYMENT=<namn-på-chattdeployment>
AZURE_OPENAI_API_VERSION=2024-10-21

# Dokumentlagring; utan dessa används en tillfällig lagring i minnet
AZURE_STORAGE_CONNECTION_STRING=<anslutningssträng>
AZURE_STORAGE_CONTAINER_NAME=<containernamn>

# Avsedda för Azure AI Search
AZURE_SEARCH_ENDPOINT=https://<sökresurs>.search.windows.net
AZURE_SEARCH_API_KEY=<api-nyckel>
AZURE_SEARCH_INDEX_NAME=<indexnamn>
AZURE_SEARCH_SEMANTIC_CONFIG=
AZURE_SEARCH_INDEXER_NAME=

# Reserverad för framtida vektorsökning; används inte ännu
AZURE_OPENAI_EMBEDDING_DEPLOYMENT=<namn-på-embeddingdeployment>
```

`AZURE_OPENAI_API_VERSION` är valfri och standardvärdet är `2024-10-21`.

Uppladdade dokument sparas i blobcontainern med filnamnet som namn och id. Om Blob Storage inte är konfigurerat används en lagring i minnet, och då försvinner dokumenten vid omstart.

## Azure AI Search

När `AZURE_SEARCH_ENDPOINT`, `AZURE_SEARCH_API_KEY` och `AZURE_SEARCH_INDEX_NAME` är satta hämtar assistenten underlaget från sökindexet, så att även PDF och Word kan besvaras. Utan dem läser assistenten bara Markdown- och textfiler direkt ur lagringen.

Uppsättning i Azure-portalen:

1. Aktivera mjuk borttagning för blobbar på lagringskontot.
2. Kör **Import data** på söktjänsten med blobcontainern som datakälla och slå på spårning av borttagningar (native blob soft delete). Guiden skapar index och indexer, och indexern knäcker PDF och Word.
3. Indexet måste ha fälten `content` (sökbar, hämtbar) och `metadata_storage_name` (hämtbar).
4. Ange indexerns namn i `AZURE_SEARCH_INDEXER_NAME`. Appen startar då indexern direkt efter varje uppladdning, ersättning och borttagning, så ändringar syns utan omstart eller manuell omindexering. Lägg gärna även ett schema på indexern (t.ex. var 5:e minut) som reserv.
5. `AZURE_SEARCH_SEMANTIC_CONFIG` är valfri; om den anges används semantisk rankning.

TIP: Hård uppdatering i din webbläsare en gång för att tömma den gamla cachade versionen. (Ctrl+Shift+R eller Ctrl+F5)