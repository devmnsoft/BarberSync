# ML Module

Pipeline inicial para reconhecimento de serviços:
1. Captura de imagem (mobile/totem)
2. Pré-processamento com OpenCV
3. Classificação com ML.NET (modelo de imagem)
4. Persistência do resultado em `ServiceRecognition`

## Endpoint alvo
`POST /api/recognition`
