import streamlit as st
import json
import azure.cognitiveservices.speech as speechsdk

# Please set up your Azure AI Speech key and region
speech_key = 'YOUR_AZURE_AI_SPEECH_KEY'
speech_region = 'YOUR_AZURE_AI_SPEECH_REGION'

st.title("Pronunciation Assessment")

def pronunciation_assessment_shortaudio(speech_key, speech_region, wav_file, reference_text, language):

    # Setup audio configuration
    audio_config = speechsdk.audio.AudioConfig(filename=wav_file)

    # Setup other speech configurations
    speech_config = speechsdk.SpeechConfig(subscription=speech_key, region=speech_region)

    ## Must disable miscue for the chatting scenario.
    json_string = {
        "GradingSystem": "HundredMark",
        "Granularity": "Phoneme",
        "EnableMiscue": False,
        "phonemeAlphabet": "IPA",
    }
    pronunciation_config = speechsdk.PronunciationAssessmentConfig(json_string=json.dumps(json_string))
    pronunciation_config.enable_prosody_assessment()
    # pronunciation_config.enable_content_assessment_with_topic(topic)
    pronunciation_config.reference_text = reference_text.strip()

    # Creates a speech recognizer using a file as audio input.
    speech_recognizer = speechsdk.SpeechRecognizer(
        speech_config=speech_config,
        language=language,
        audio_config=audio_config
    )
    # Apply pronunciation assessment config to speech recognizer
    pronunciation_config.apply_to(speech_recognizer)

    # Recognize using 'once'
    return speech_recognizer.recognize_once_async().get()

file = st.selectbox('Select audio file', ['What is the weather like? (en-US)', '本日は晴天なり (ja-JP)'])
if file != None: 
    if file == 'What is the weather like? (en-US)':
        wav_file = './wav/whatstheweatherlike.wav'
        language = 'en-US'
        reference_text = 'what is the weather like?'
    elif file == '本日は晴天なり (ja-JP)':
        wav_file = './wav/Itsfinetoday.wav'
        language = 'ja-JP'
        reference_text = '本日は晴天なり'

    st.audio(wav_file)
    st.text('Reference text: ' + reference_text)
    st.text('Audio file: ' + wav_file)
    st.text('Language: ' + language)


if st.button('Submit'):
    result = pronunciation_assessment_shortaudio(speech_key, speech_region, wav_file, reference_text, language)
    st.write(result.reason)
    
    if result.reason == speechsdk.ResultReason.RecognizedSpeech:
        resultJson = json.loads(result.json)
        st.write(('Text: ' + resultJson['DisplayText']))
        st.write(resultJson['NBest'][0]['PronunciationAssessment'])