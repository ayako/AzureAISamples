import streamlit as st
import json
import azure.cognitiveservices.speech as speechsdk
from assessment import pronunciation_assessment_shortaudio, pronunciation_assessment_longaudio

st.title("Pronunciation Assessment")

file = st.selectbox('Select audio file', 
                    ['What is the weather like? (en-US)', '本日は晴天なり (ja-JP)', 'Season - Fall (en-US)', '雪国 (ja-JP)'])
if file != None: 
    if file == 'What is the weather like? (en-US)':
        wav_file = './wav/whatstheweatherlike.wav'
        type = 'short'
        language = 'en-US'
        reference_text = 'what is the weather like?'
    elif file == '本日は晴天なり (ja-JP)':
        wav_file = './wav/itsfinetoday.wav'
        type = 'short'
        language = 'ja-JP'
        reference_text = '本日は晴天なり'
    elif file == 'Season - Fall (en-US)':
        wav_file = './wav/season_fall.wav'
        type = 'long'
        language = 'en-US'
        reference_text = 'The fall is a time of cozy contemplation for some, a sad time for others. No longer the far-flung ecstasy of summer and preceding the hibernation period of winter, it is a time of slowing down to reflect. It is no wonder the fall is called the spring of the philosopher. This season confronts us with what we have cultivated within. The ancients of many traditions honored this sacred time by taking autumns rich external symbolism as a tool for in a rebirth.'

    elif file == '雪国 (ja-JP)':
        wav_file = './wav/yukiguni.wav'
        type = 'long'
        language = 'ja-JP'
        reference_text = '国境の長いトンネルを抜けると雪国であった。夜の底が白くなった。信号所に汽車が止まった。向側の座席から娘が立って来て、島村の前のガラス窓を落した。雪の冷気が流れこんだ。娘は窓いっぱいに乗り出して、遠くへ呼ぶように、「駅長さあん、駅長さあん」明りをさげてゆっくり雪を踏んで来た男は、襟巻で鼻の上まで包み、耳に帽子の毛皮を垂れていた。もうそんな寒さかと島村は外を眺めると、鉄道の官舎らしいバラックが山裾に寒々と散らばっているだけで、雪の色はそこまで行かぬうちに闇に呑まれていた。「駅長さん、私です、御機嫌よろしゅうございます」「ああ、葉子さんじゃないか。お帰りかい。また寒くなったよ」'

    st.audio(wav_file)
    st.text('Reference text: ' + reference_text)
    st.text('Audio file: ' + wav_file)
    st.text('Type: ' + type)
    st.text('Language: ' + language)


if st.button('Submit'):
    if type == 'short':
        result = pronunciation_assessment_shortaudio(language, wav_file, reference_text)
    
        if result.reason == speechsdk.ResultReason.RecognizedSpeech:
            st.write('Success')

            resultJson = json.loads(result.json)
            st.write(('Text: ' + resultJson['DisplayText']))
            st.write(resultJson['NBest'][0]['PronunciationAssessment'])

    elif type == 'long':
        result = pronunciation_assessment_longaudio(language, wav_file, reference_text)
    
        if len(result) > 1:
            st.write('Success')
            result_scores = [ {'accuracy_score': x.accuracy_score, 
                          'prosody_score': x.prosody_score, 
                          'pronunciation_score': x.pronunciation_score, 
                          'completeness_score': x.completeness_score, 
                          'fluency_score': x.fluency_score} for x in result ]
            result_overall_score = { 'accuracy_score': sum([x.accuracy_score for x in result])/len(result),
                                     'prosody_score': sum([x.prosody_score for x in result])/len(result),
                                     'pronunciation_score': sum([x.pronunciation_score for x in result])/len(result),
                                     'completeness_score': sum([x.completeness_score for x in result])/len(result),
                                     'fluency_score': sum([x.fluency_score for x in result])/len(result)}
            st.write({'overall_score': result_overall_score, 'indivitual_scores': result_scores})
