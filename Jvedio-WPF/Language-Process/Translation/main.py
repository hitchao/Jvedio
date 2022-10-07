import os

origin_lang = "zh-CN"
target_lang = ["en-US", "ja-JP"]


def print_key_value():
    path = os.path.join(os.getcwd(), "Translation", "origin.xaml")
    content = open(path, 'r', encoding='utf8').read()
    dict = {}
    key_output = ""
    for item in content.split('\n'):
        if 'x:Key="' in item:
            start = item.index('x:Key="')
            end = item.index('">')
            key = item[start + 7:end]
            key_output += f"{key}\n"
            v_end = item.index('</sys:String>')
            value = item[end + 2:v_end]
            dict[key] = value
    open(os.path.join(os.getcwd(), "Translation", "Lang", "key.txt"),
         'w',
         encoding='utf8').write(key_output)
    for lang in target_lang:
        value_output = ""
        for key in dict.keys():
            value = dict[key]
            # 输出 value
            value_output += f"{value}\n"
        value_output_file_name = os.path.join(os.getcwd(), "Translation",
                                              "Lang", f"{lang}.txt")
        open(value_output_file_name, 'w', encoding='utf8').write(value_output)


def generate_xaml():
    key_path = os.path.join(os.getcwd(), "Translation", "Lang", "key.txt")
    keys = open(key_path, 'r', encoding='utf8').read().split('\n')
    for lang in target_lang:
        output = ""
        lang_path = os.path.join(os.getcwd(), "Translation", "Value",
                                 f"{lang}.txt")
        content = open(lang_path, 'r', encoding='utf8').read().split('\n')
        l = len(content)
        for i in range(l):
            if len(keys[i]) > 0 and len(content[i]) > 0:
                key = keys[i]
                value = content[i]
                line = f'<sys:String x:Key="{key}">{value}</sys:String>\n'
                output += line
        output_file_name = os.path.join(os.getcwd(), "Translation", "Output",
                                        lang + ".xaml")
        open(output_file_name, 'w', encoding='utf8').write(output)


if __name__ == "__main__":
    #  print_key_value()
    generate_xaml()