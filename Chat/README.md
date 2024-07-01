# Azure OpenAI Service Chat Sample

## Notebook

Step by Step で Azure OpenAI Service の設定、シングルチャットのやり取り、マルチターンでのチャットのやり取りを記載しています。
以下の Azure OpenAI Service のエンドポイント(URL) と API キーを設定してください。

```
## Azure OpenAIのエンドポイント(URL),Azure OpenAIのAPIキー を設定
client = AzureOpenAI(
    azure_endpoint="https://YOUR_SERVICE_NAME.openai.azure.com/",
    api_key="YOUR_API_KEY",
    api_version="2024-02-01",
)
```

```
## GPTモデルのデプロイ名前を設定
GPT_MODEL = "gpt-4o"
```

# WebUI

複数の Azure OpenAI Service のデプロイモデルを利用できるシンプルなチャット用Web画面です。ローカルで HTML と Javascript で稼働します (Web サーバー不要)。

以下の Azure OpenAI Service のエンドポイント(URL) と API キー、各 OpenAI モデルのデプロイ名を設定してください。


script.js
```
// set your Azure OpenAI endpoint and API key
const AOAI_BASE_URL = "https://YOUR_SERVICE_NAME.openai.azure.com/";
const API_KEY = "YOUR_API_KEY";
```
index.html
```
<!-- set your Azure OpenAI deployment name correctly at "data-model" -->
<button class="model-button" data-model="gpt-35-turbo-16k">GPT-3.5 Turbo</button>  
<button class="model-button" data-model="gpt-4">GPT-4</button>  
<button class="model-button" data-model="gpt-4-vision">GPT-4 vision</button>  
<button class="model-button" data-model="gpt-4o">GPT-4o</button>
```  