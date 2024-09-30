from openai import AzureOpenAI
import os
import json
import requests
from datetime import datetime
from dotenv import load_dotenv

def create_image(prompt):

    load_dotenv()

    client = AzureOpenAI(
        azure_endpoint=os.getenv("AOAI_DALLE3_ENDPOINT"),
        api_key=os.getenv("AOAI_DALLE3_KEY"),
        api_version=os.getenv("AOAI_DALLE3_API_VERSION"),
    )

    result = client.images.generate(
        model=os.getenv("AOAI_DALLE3_DEPLOYMENT_NAME"),
        prompt=prompt,
        n=1
    )

    image_url = json.loads(result.model_dump_json())['data'][0]['url']

    if image_url == "":
        return json.loads(result.model_dump_json())

    else:
        response = requests.get(image_url)
        if response.status_code == 200:
            file_name = "./images/" + datetime.now().strftime("%Y%m%d%H%M%S")+".png"
            with open(file_name, "wb") as f:
                f.write(response.content)
            # return (f"created: " + file_name)
            return file_name

        else:
            # return (f"Failed to download image. Status code: {response.status_code}")
            return ""
