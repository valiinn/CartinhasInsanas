# 📘 Guia de Contribuição — CartinhasInsanas (Unity + Git)

Repositório: `https://github.com/valiinn/CartinhasInsanas.git`

Este guia explica **do zero** como:
1) **Clonar** o projeto do GitHub
2) **Abrir** no Unity
3) **Criar sua branch** de trabalho
4) **Enviar mudanças** para o GitHub (push) e **trazer atualizações** (pull)
5) Boas práticas para trabalhar em equipe com Unity + Git

---

## ✅ Pré‑requisitos (instalar uma vez)
- **Git for Windows**: https://git-scm.com/download/win  
- **Unity Hub** instalado e a **mesma versão do Unity** do projeto.  
  - Para checar a versão do projeto: após clonar, abra o arquivo `ProjectSettings/ProjectVersion.txt` (texto).  
- (Opcional) **VS Code** ou **Visual Studio** para editar scripts.

> Dica Windows: ative caminhos longos (evita erro “Filename too long”):
```bash
git config --global core.longpaths true
```

> Dica de login no GitHub: o **Git Credential Manager** costuma abrir uma janelinha para login quando você fizer `git push`. Se não abrir:
```bash
git config --global credential.helper manager
```

---

## 🗂️ 1) Escolha/Crie uma pasta de trabalho
Você pode trabalhar, por exemplo, em `C:\UnityProjects`.

1. Abra o **Explorador de Arquivos**
2. Crie a pasta `C:\UnityProjects` (ou outra de sua preferência)

---

## 🐧 2) Abrir o Git Bash **na pasta escolhida**
### Opção A — Clique direito
- Vá para `C:\UnityProjects`
- **Clique com o botão direito** na área vazia → **Git Bash Here**

### Opção B — Abrir e navegar pelo terminal
- Abra o **Git Bash** pelo menu Iniciar
- Navegue até a pasta com `cd` (use aspas se houver espaços no caminho):
```bash
cd "C:\UnityProjects"
```
- Confira onde você está:
```bash
pwd
```
- Liste arquivos/pastas (para conferir):
```bash
ls
```

---

## ⬇️ 3) Clonar o repositório
No **Git Bash**, já dentro da pasta de trabalho (ex.: `C:\UnityProjects`), execute:

```bash
git clone https://github.com/valiinn/CartinhasInsanas.git
```

Isso criará a pasta `CartinhasInsanas`. Entre nela:

```bash
cd CartinhasInsanas
```

> Se for seu **primeiro uso** do Git na máquina, configure seu nome e e‑mail (apenas uma vez):
```bash
git config --global user.name "Seu Nome"
git config --global user.email "seuemail@exemplo.com"
```

---

## 🎮 4) Abrir o projeto no Unity Hub
1. Abra o **Unity Hub**
2. Clique em **Add** (Adicionar)
3. Selecione a pasta que você clonou: `C:\UnityProjects\CartinhasInsanas`
4. Abra o projeto.  
   - Se o Hub pedir para instalar outra versão do Unity, **instale a mesma versão do projeto** (confira em `ProjectSettings/ProjectVersion.txt`). Evite atualizar a versão do projeto sem alinhar com o time.

> Recomendações de projeto para Git (já devem estar assim, mas confira):  
**Edit → Project Settings → Editor**  
- **Version Control**: _Visible Meta Files_  
- **Asset Serialization**: _Force Text_

Se você mudar algo nessas opções, **salve** e depois faça um commit (ver seção de commits).

---

## 🌱 5) Criar sua **branch** de trabalho
Nunca trabalhe direto na `main`. Crie sua própria branch:

```bash
# Garanta que você está dentro da pasta do projeto clonado
cd "C:\UnityProjects\CartinhasInsanas"

# Veja em qual branch está
git status

# Baixe as últimas referências do repositório
git fetch origin

# Crie e troque para sua branch (exemplos de nomes):
git checkout -b luiz/ui-inventario
# ou
git checkout -b seu-nome/tarefa-resumo
```

> **Padrão sugerido de nomes**: `seu-nome/assunto-curto`  
Ex.: `ana/sistema-cartas`, `joao/cena-menu`

Primeiro push (define o “upstream”):

```bash
git push -u origin luiz/ui-inventario
# substitua pelo nome da sua branch
```

Depois disso, basta `git push` nas próximas vezes.

---

## 💾 6) Fazer alterações, **commitar** e **enviar** (push)
Fluxo típico após editar algo no Unity ou nos scripts:

```bash
# Veja o que mudou
git status

# Adicione tudo que deseja versionar
git add .

# Faça um commit com mensagem clara (no imperativo, curta e objetiva)
git commit -m "Cria sistema de compra de cartas na loja"

# Envie sua branch para o GitHub
git push
```

> **Somente** arquivos importantes devem ir para o Git (e o `.gitignore` já cuida do resto):  
- `Assets/` (cenas, prefabs, scripts, etc.)  
- `Packages/`  
- `ProjectSettings/`  
> **NÃO** versionamos: `Library/`, `Temp/`, `Logs/`, `Obj/`, `Build/` etc.

---

## 🔄 7) Atualizar seu trabalho com o que está na `main`
Antes de começar o dia (ou abrir um Pull Request), traga atualizações da `main` para sua branch:

```bash
# 1) Ir para a main e atualizar
git checkout main
git pull origin main

# 2) Voltar para sua branch e mesclar a main nela
git checkout luiz/ui-inventario
git merge main
# Resolva conflitos se houver (ver abaixo) e depois:
git add .
git commit -m "Resolve conflitos da merge com main"
```

> Alternativa (mais avançada): `git rebase main` na sua branch. Para iniciantes, **prefira `merge`**.

---

## 🔀 8) Abrir um Pull Request (PR)
1. Vá para: `https://github.com/valiinn/CartinhasInsanas`
2. O GitHub costuma sugerir abrir um PR da sua branch → **Compare & pull request**
3. Base: `main` ← compare: `sua-branch`
4. Título e descrição do que foi feito
5. Marque colegas como reviewers
6. Após aprovação, **mergeie** o PR (evite dar merge na sua própria mudança sem revisão, se possível)

---

## 🧩 9) Resolvendo conflitos (no básico)
Se aparecer “conflict” ao fazer `git merge main`:
1. O Git mostrará os arquivos em conflito (`git status`)
2. Abra os arquivos e procure marcações `<<<<<<<`, `=======`, `>>>>>>>`
3. Escolha o que **deve ficar**, apague as marcações, salve
4. Rode:
```bash
git add .
git commit -m "Resolve conflitos em X e Y"
```
5. Teste o projeto no Unity antes de fazer `git push`

> **Evite** que duas pessoas editem a **mesma cena** simultaneamente. Prefira trabalhar em cenas/prefabs separados ou combine horários.

---

## 🧠 10) Boas práticas de commit
- Faça **commits pequenos** e frequentes
- Mensagens no **imperativo** e claras:  
  - `Adiciona lógica de dano crítico`  
  - `Ajusta posição da câmera no topo`  
- Evite commits com “tudo de uma vez”

---

## 🆘 11) Problemas comuns
- **`Filename too long`**  
  Rode: `git config --global core.longpaths true` e **não** versione `Library/`.

- **`fatal: not a git repository`**  
  Você não está dentro da pasta do projeto. Rode `cd CartinhasInsanas`.

- **Erro de permissão com SSH**  
  Use o **HTTPS** do repositório (o que está neste guia) para evitar chave SSH no início.

- **Cena quebrada após merge**  
  Abra a cena, verifique o _Hierarchy_ e _Inspector_, teste no Play. Às vezes é necessário refazer um ajuste manual.

---

## 🧭 Resumo rápido (cola)
```bash
# Escolha pasta e abra Git Bash nela
cd "C:\UnityProjects"

# Clone o repo e entre na pasta
git clone https://github.com/valiinn/CartinhasInsanas.git
cd CartinhasInsanas

# (uma vez) configure seu nome/email
git config --global user.name "Seu Nome"
git config --global user.email "seuemail@exemplo.com"

# Crie sua branch e envie
git checkout -b seu-nome/minha-tarefa
git push -u origin seu-nome/minha-tarefa

# Trabalhe, faça commits e push
git add .
git commit -m "Mensagem curta e clara"
git push

# Atualize sua branch com a main
git checkout main
git pull origin main
git checkout seu-nome/minha-tarefa
git merge main
```

---

Se algo não bater, manda print/erro no grupo que ajudamos. Bom trabalho! 💪🎮
