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

Uppladdade dokument sparas i blobcontainern med filnamnet som namn och id. Om Blob Storage inte är konfigurerat används en lagring i minnet, och då försvinner dokumenten vid omstart. Aktivera mjuk borttagning för blobbar på lagringskontot så att AI Search kan upptäcka borttagna dokument. AI Search är ännu inte inkopplat i koden, dess variabler används för närvarande bara för att visa konfigurationsstatus.

TIP: Hård uppdatering i din webbläsare en gång för att tömma den gamla cachade versionen. (Ctrl+Shift+R eller Ctrl+F5)