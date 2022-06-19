import os
from M2Crypto import EVP
key = EVP.Cipher(alg='aes_128_cbc', key=os.urandom(16), iv=os.urandom(16), op=enc)
print(key)