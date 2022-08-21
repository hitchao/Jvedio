import os
import re
xaml_path = "D:\Visual_Studio_Project\chao-controls\SuperControls.Style\XAML\Skin\DefaultColor.xaml"
output_json = os.path.join(os.getcwd(), "output.json")
d = {}
key_list=[]

def Parse():
    with open(xaml_path, 'r', encoding='utf-8') as f:
        text = f.read()
        for line in text.split('\n'):
            find_key = re.search(r'(?<=\bx:Key=")[^"]*', line)
            find_color = re.search(r'(?<=\bColor=")[^"]*', line)
            if find_key:
                key_list.append(find_key.group(0))
                if find_color:
                    d[find_key.group(0)] = find_color.group(0)
                else:
                    idx1 = str.find(line, ">")
                    line = line[idx1 + 1:]
                    idx2 = str.find(line, "<")
                    color=line[:idx2]
                    if color:
                        d[find_key.group(0)] = color


def formatPrintKeyList():
    print("{")
    for item in key_list:
        print(f'  "{item}",')
    print("};")



def FormatPrintColorList():
    open('color_output.txt','w').write("")
    with open('color_output.txt','a') as f:
        f.write("{\n")
        for k in d:
            f.write("    {" +f'"{k}","{d[k]}"'+"},\n")
        f.write("\n};")

Parse()
print(d)
#formatPrintKeyList()
#FormatPrintColorList()