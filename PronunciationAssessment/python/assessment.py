import json
import time
import azure.cognitiveservices.speech as speechsdk

# Please set up your Azure AI Speech key and region
speech_key = 'YOUR_AZURE_AI_SPEECH_KEY'
speech_region = 'YOUR_AZURE_AI_SPEECH_REGION'

def set_speech_recognizer(language, wav_file, reference_text):

    # Setup audio configuration
    audio_config = speechsdk.audio.AudioConfig(filename=wav_file)

    # Setup other speech configurations
    speech_config = speechsdk.SpeechConfig(subscription=speech_key, region=speech_region)

    # Creates a speech recognizer using a file as audio input.
    speech_recognizer = speechsdk.SpeechRecognizer(
        speech_config=speech_config,
        language=language,
        audio_config=audio_config
    )

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

    # Apply pronunciation assessment config to speech recognizer
    pronunciation_config.apply_to(speech_recognizer)

    return speech_recognizer


def pronunciation_assessment_shortaudio(language, wav_file, reference_text):

    speech_recognizer = set_speech_recognizer(language, wav_file, reference_text)

    # Recognize using 'once'
    return speech_recognizer.recognize_once_async().get()


def pronunciation_assessment_longaudio(language, wav_file, reference_text):

    speech_recognizer = set_speech_recognizer(language, wav_file, reference_text)

    # Prep for continuous recognition
    done = False
    assessment_result = []

    def stop_cb(evt: speechsdk.SessionEventArgs):
        """callback that signals to stop continuous recognition upon receiving an event `evt`"""
        nonlocal done
        done = True

    def recognized(evt: speechsdk.SpeechRecognitionEventArgs):
        pronunciation_result = speechsdk.PronunciationAssessmentResult(evt.result)
        assessment_result.append(pronunciation_result)

    # Connect callbacks to the events fired by the speech recognizer
    speech_recognizer.recognized.connect(recognized)
    speech_recognizer.session_started.connect(lambda evt: print('SESSION STARTED: {}'.format(evt)))
    speech_recognizer.session_stopped.connect(lambda evt: print('SESSION STOPPED {}'.format(evt)))
    speech_recognizer.canceled.connect(lambda evt: print('CANCELED {}'.format(evt)))

    # Stop continuous recognition on either session stopped or canceled events
    speech_recognizer.session_stopped.connect(stop_cb)
    speech_recognizer.canceled.connect(stop_cb)

    # Start continuous pronunciation assessment
    speech_recognizer.start_continuous_recognition()
    while not done:
        time.sleep(.5)

    speech_recognizer.stop_continuous_recognition()

    # return assessment_result, assessment_result_json
    return assessment_result
