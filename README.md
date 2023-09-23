# Steganography With Encryption
Enables users to encode and decode messages within image files using steganography and Vigenère encryption. Load a raw image of any size, type a message, specify a keyword, and perform the encoding or decoding operation. The message is first encrypted using the Vigenère polyalphabetic cipher with the specified keyword, then hidden in the image's least significant bits. When encoding, there is a check for maximum characters, based upon the bytes available in the image. 

![input](https://github.com/akingry/SteganographyWithEncryption/assets/111338740/2cfefa8f-9f90-452f-8418-1f105d5d2553)

When decoding, the message is extracted from the image's pixels, and then decrypted using the same cipher to reveal the original message. There is no visible difference between the raw image and encoded image. 

![out1](https://github.com/akingry/SteganographyWithEncryption/assets/111338740/cdc4ef0d-3c8d-4393-8a00-c96a44c30210)

The encoded message cannot be decrypted without using the same keyword which was used to enctypt it. Without the proper keyword, the decoded text remains indecipherable.

![image](https://github.com/akingry/SteganographyWithEncryption/assets/111338740/1dc929f2-b360-43de-8aec-446001dbb876)

