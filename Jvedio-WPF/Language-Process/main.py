import os
import xml.etree.ElementTree as ET

if __name__ == "__main__":
    prefix = open("sample.txt", 'r', encoding='utf8').read()
    path_list = [
        "Resources.en-US.resx", "Resources.ja-JP.resx", "Resources.zh-CN.resx"
    ]
    data = {"en-US": {}, "ja-JP": {}, "zh-CN": ""}
    for p in path_list:
        n = p.replace("Resources.", "").replace(".resx", "")
        path = os.path.join(os.getcwd(), p)
        root = ET.parse(path).getroot()
        dict = {}
        for data_tag in root.findall('data'):
            name = data_tag.get('name')
            value = data_tag.findall('value')[0].text
            dict[name] = value
        data[n] = dict
        # 输出到 DynamicResource
        output = prefix
        for key in dict.keys():
            line = f'<sys:String x:Key="{key}">{dict[key]}</sys:String>\n'
            output += line
        output += "</ResourceDictionary>"
        output_file_name = os.path.join(os.getcwd(), "Lang", n + ".xaml")
        open(output_file_name, 'w', encoding='utf8').write(output)
    #open("lang.json",'w',encoding='utf8').write(str(data))
