# 🥽 Projeto VR - Guia de Execução (Unity)

Este guia explica como configurar e executar corretamente o projeto VR na Unity.

---

## 📦 Requisitos

- Unity Hub instalado
- Versão exata da Unity usada no projeto
- Git instalado
- (Opcional) SDKs de VR (Meta Quest / OpenXR)

---

## 🚀 1. Clonar o projeto

Clone o repositório com o Git:

    git clone <url-do-repositorio>

Depois disso, abra a pasta do projeto.

---

## 🧠 2. Usar a versão correta da Unity

Verifique a versão da Unity no arquivo:

`ProjectSettings/ProjectVersion.txt`

Exemplo de conteúdo:

    m_EditorVersion: 2022.3.10f1

⚠️ **IMPORTANTE:**  
Use exatamente essa versão no Unity Hub para evitar erros.

---

## 🖥️ 3. Abrir o projeto

1. Abra o Unity Hub  
2. Clique em **"Add" (Adicionar projeto)**  
3. Selecione a pasta do projeto  
4. Abra usando a versão correta da Unity  

---

## ⏳ 4. Primeira execução

Na primeira vez que abrir:

- A Unity vai recriar a pasta `Library/`
- Vai importar todos os assets
- Pode demorar alguns minutos

👉 Isso é normal!

---

## 📦 5. Pacotes e dependências

O projeto já contém os pacotes necessários em:

`Packages/manifest.json`

Se algo não funcionar:

1. Vá em **Window → Package Manager**
2. Verifique se há erros ou pacotes faltando

---

## 🥽 6. Configuração de VR (IMPORTANTE)

Como este é um projeto VR, verifique:

### XR Plug-in Management

1. Vá em:

   `Edit → Project Settings → XR Plug-in Management`

2. Certifique-se de que:
   - OpenXR está ativado
   - Plataforma correta selecionada (PC ou Android)

---

### XR Interaction Toolkit

- Verifique se está instalado no Package Manager
- Necessário para interação VR (teleporte, mãos, etc.)

---

## 📱 7. Build Settings

1. Vá em:

   `File → Build Settings`

2. Configure:
   - Plataforma correta (PC ou Android)
   - Cena principal adicionada

---

## ⚠️ Problemas comuns

### ❌ Projeto não abre
- Verifique se a versão da Unity está correta

### ❌ Erros no console
- Falta de pacotes
- Configuração XR incorreta

### ❌ Teleporte ou input não funciona
- Verifique XR Interaction Toolkit
- Verifique configuração de Input System

### ❌ Cena vazia
- Certifique-se de que a cena correta está aberta

---

## 🚫 8. Arquivos que NÃO devem ser enviados ao Git

Nunca adicione ao repositório:

    Library/
    Temp/
    Logs/
    Build/

Essas pastas são geradas automaticamente pela Unity.

---

## ✅ 9. Dicas finais

- Sempre use a mesma versão da Unity do projeto
- Aguarde o carregamento completo na primeira execução
- Em caso de erro, verifique o Console e o Package Manager
- Para VR, sempre valide as configurações de XR

---

## 🤝 Suporte

Se algo não funcionar:

👉 Entre em contato com o responsável pelo projeto
