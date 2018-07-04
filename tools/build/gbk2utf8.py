#! python2
#encoding:utf-8

import codecs
import traceback
import os
import sys

def convert_cpp_file(rootPath,totalFile):
    for file in os.listdir(rootPath):
        fullName = rootPath + "/" + file
        if(os.path.isdir(file)):
            convert_cpp_file(fullName,totalFile)
        else:
            file_array = file.split(".")
            if(len(file_array) > 1):
                fileExt = file_array[1]
                if(fileExt == "cpp" or fileExt == "h" or fileExt == "cc" or fileExt == "c" or fileExt == "hpp"):
                    fileObj = open(fullName,"rb")
                    #不是UTF-8签名文件,按照GBK处理
                    if fileObj.read(3) != "\xef\xbb\xbf":
                        fileObj.close()
                        fileObj = codecs.open(fullName,"r","gbk")
                        fileBytes = fileObj.read()
                        fileObj.close()
                        fileObj = codecs.open(fullName,"w","utf-8-sig")
                        fileObj.write(fileBytes)
                        fileObj.close()
                        totalFile[0] = totalFile[0] + 1
if(__name__ == "__main__"):
    try:
        if(len(sys.argv) >= 2):
            totalFile = [0]
            convert_cpp_file(os.path.abspath(sys.argv[1]),totalFile)
            print(u"转换结束 共转换" + unicode(totalFile[0]) + u"个文件")
        else:
            print(u"没有设置转换根目录!")
        os.system("pause")
    except:
        traceback.print_exc()
        os.system("pause")