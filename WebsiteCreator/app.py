import streamlit as st
from promptflow.core import Prompty
import json
import os
from painter import create_image

def set_page_properties(data, imageUrl):
    with open("./page/script_base.js", 'r', encoding='utf-8') as file:
        contents = file.read();
    with open("./page/script.js", 'w', encoding='utf-8') as file:
        file.write("const parameters =" + json.dumps(data, ensure_ascii=False) + ";\n" + contents)

    with open(imageUrl, 'rb') as src_file:
        with open("./page/image01.png", 'wb') as dst_file:
            dst_file.write(src_file.read())
    

default_input = "私は「NPO法人 立山のライチョウを守る会」を主催しています。国の天然記念物に指定されているライチョウは、絶滅危惧種に指定されています。そのため、外敵からの保護および繁殖を助けるための活動を行いたいと考えています。集めた資金は、ライチョウを保護し、繁殖するために使います。目標金額は1000万円です。来年度の活動に充てるため、2025年4月までに目標金額を達成したいです。また、活動は2025年10月末を予定しています。夜間にライチョウを保護するための安全柵の設置に500万円、繁殖のための環境整備に500万円、を充てる予定です。ご支援の方法としては、一口¥5,000で、ライチョウの写真やグッズをお送りするプランがあります。リターンの配送は2025年6月を予定しています。"

st.title("クラウドファンディング募集ページ作成")
st.write("クラウドファンディング募集ページの作成をサポートします。")
user_input = st.text_area("プロジェクトの概要を入力してください", value=default_input, height=250)

if st.button("submit"):

    # Extract and generate text information
    editor_flow = Prompty.load("./editor.prompty")
    text_result = editor_flow(
        question = user_input
    )
    try:
        result_json = json.loads(text_result)
    except json.JSONDecodeError:
        st.write("情報の抽出、生成ができませんでした。しばらく時間をおいて再度お試しください。")
        st.stop()

    result_feedbacK = result_json.get("feedback")
    result_data = result_json.get("data")

    # Generate image
    painter_flow = Prompty.load("./painter.prompty")
    painter_prompt = painter_flow(
        question = user_input
    )

    image_result = create_image(painter_prompt)
    if image_result == "":
        st.write("画像の生成ができませんでした。しばらく時間をおいて再度お試しください。")
        st.stop()

    # Show results
    st.write("改善点:")
    st.write(result_feedbacK)

    st.write("データ:")
    st.text_input("ページタイトル", value=result_data.get("title"))
    st.text_input("リード文", value=result_data.get("project_summary"))
    st.text_input("概要", value=result_data.get("project_purpose"))
    st.text_input("目標", value=result_data.get("project_goal"))
    st.text_input("主催者", value=result_data.get("project_organizer"))
    st.text_input("目標金額", value=result_data.get("target_amount"))
    st.text_input("資金の用途", value=result_data.get("fund_usage"))
    st.text_input("資金の用途の内訳", value=result_data.get("fund_usage_breakdown"))
    st.text_input("期間", value=result_data.get("fund_term"))
    st.text_input("プラン名", value=result_data.get("return01_title"))
    st.text_input("プランの金額", value=result_data.get("return01_amount"))
    st.text_input("リターンの配布時期", value=result_data.get("return01_delivery"))
    st.text_input("プランの詳細", value=result_data.get("return01_detail"))
    st.text_input("スケジュール", value=result_data.get("schedule"))

    st.text_area("画像生成プロンプト", value=painter_prompt, height=200)
    st.image(image_result)

    # Show preview
    set_page_properties(result_data, image_result)
    htmlPath = os.path.join(os.path.dirname(__file__), "./page/index.html")
    st.markdown(f'<a href="file://{htmlPath}" target="_blank">生成されたページのリンク</a>', unsafe_allow_html=True)