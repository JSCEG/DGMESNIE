%!TEX TS-program = xelatex
% Clase institucional para informes SENER.
% Sistema de diseno: Documentos latex/05_Sistema_de_Diseno
% Version: 2.9 - 14 de julio de 2026

\NeedsTeXFormat{LaTeX2e}
\ProvidesClass{institucional}[2026/07/14 v2.9 Clase institucional SENER para informes]

\LoadClass[10pt,letterpaper]{article}

\RequirePackage{iftex}
\ifXeTeX\else
  \ClassError{institucional}{Esta clase requiere XeLaTeX}{Compile con XeLaTeX para usar las fuentes institucionales incluidas.}
\fi

\RequirePackage[spanish,es-nodecimaldot]{babel}
\RequirePackage{fontspec}
\RequirePackage{microtype}
\RequirePackage[letterpaper,top=2.55cm,bottom=2.45cm,left=2.25cm,right=2.25cm,headheight=29pt,headsep=13pt,footskip=26pt]{geometry}
\RequirePackage{graphicx}
\RequirePackage[table]{xcolor}
\RequirePackage{array}
\RequirePackage{longtable}
\RequirePackage{booktabs}
\RequirePackage{tabularx}
\RequirePackage{multirow}
\RequirePackage{ragged2e}
\RequirePackage{enumitem}
\RequirePackage{titlesec}
\RequirePackage{tocloft}
\RequirePackage{fancyhdr}
\RequirePackage{caption}
\RequirePackage{etoolbox}
\RequirePackage{eso-pic}
\RequirePackage{tikz}
\usetikzlibrary{calc,positioning,arrows.meta,shapes.geometric,fit,backgrounds}
\RequirePackage[most]{tcolorbox}
\RequirePackage{fancyvrb}
\RequirePackage{xparse}

% El entorno local puede desactivar enlaces para una prueba de maquetacion.
% Prism debe compilar sin definir NoHyperref.
\ifdefined\NoHyperref
  \providecommand{\href}[2]{#2}
  \providecommand{\url}[1]{#1}
  \providecommand{\hyperlink}[2]{#2}
  \providecommand{\hypertarget}[2]{#2}
  \providecommand{\phantomsection}{}
  \providecommand{\hypersetup}[1]{}
\else
  \RequirePackage{hyperref}
  \RequirePackage{xurl}
\fi
\providecommand{\XeTeXLinkBox}[1]{#1}

% --------------------------------------------------------------------------
% Paleta institucional
% --------------------------------------------------------------------------
\definecolor{SENERGuinda}{HTML}{9B2247}
\definecolor{SENERGuindaOscuro}{HTML}{8E1F40}
\definecolor{SENERDorado}{HTML}{E0A12E}
\definecolor{SENERDoradoSuave}{HTML}{C9A24B}
\definecolor{SENEREsmeralda}{HTML}{0E8A6E}
\definecolor{SENERCian}{HTML}{1E9CB8}
\definecolor{SENERTinta}{HTML}{1C1B1A}
\definecolor{SENERGris}{HTML}{6F6B66}
\definecolor{SENERGrisClaro}{HTML}{9A958E}
\definecolor{SENERPapel}{HTML}{FAF8F5}
\definecolor{SENERLinea}{HTML}{ECE8E2}
\definecolor{SENERLineaSuave}{HTML}{F0ECE6}
\definecolor{SENERPositivoFondo}{HTML}{E4F2EC}

% Alias para documentos heredados.
\colorlet{SENER}{SENERGuinda}
\colorlet{SENERGold}{SENERDorado}
\colorlet{TextGray}{SENERGris}

% --------------------------------------------------------------------------
% Tipografia incluida en el proyecto
% --------------------------------------------------------------------------
\defaultfontfeatures{Ligatures=TeX,Scale=MatchLowercase}

\IfFileExists{fonts/LibreFranklin-Regular.otf}{
  \setmainfont[
    Path=fonts/,
    UprightFont=LibreFranklin-Regular.otf,
    ItalicFont=LibreFranklin-Italic.otf,
    BoldFont=LibreFranklin-SemiBold.otf,
    BoldItalicFont=LibreFranklin-Italic.otf
  ]{Libre Franklin}
  \setsansfont[
    Path=fonts/,
    UprightFont=LibreFranklin-Regular.otf,
    ItalicFont=LibreFranklin-Italic.otf,
    BoldFont=LibreFranklin-SemiBold.otf,
    BoldItalicFont=LibreFranklin-Italic.otf
  ]{Libre Franklin}
}{
  \setmainfont{TeX Gyre Heros}
  \setsansfont{TeX Gyre Heros}
}

\IfFileExists{fonts/Patria_Regular.otf}{
  \newfontfamily\senerdisplay[
    Path=fonts/,
    UprightFont=Patria_Regular.otf,
    BoldFont=Patria_Bold.otf,
    ItalicFont=Patria_Light.otf
  ]{Patria}
}{
  \newfontfamily\senerdisplay{TeX Gyre Pagella}
}

\IfFileExists{fonts/IBMPlexMono-Regular.otf}{
  \setmonofont[
    Path=fonts/,
    UprightFont=IBMPlexMono-Regular.otf,
    BoldFont=IBMPlexMono-SemiBold.otf
  ]{IBM Plex Mono}
}{
  \setmonofont{Latin Modern Mono}
}

\newcommand{\senertexto}{\normalfont}
\newcommand{\senermono}{\ttfamily}
\newcommand{\senerkickerfont}{\sffamily\bfseries\fontsize{8.5pt}{10.5pt}\selectfont\addfontfeatures{LetterSpace=12}}
\newcommand{\senermicro}{\sffamily\fontsize{7.5pt}{9.5pt}\selectfont}
\newcommand{\senercaption}{\sffamily\fontsize{8pt}{10pt}\selectfont}

% --------------------------------------------------------------------------
% Metadatos
% --------------------------------------------------------------------------
\makeatletter
\newcommand{\@subtitle}{}
\newcommand{\subtitle}[1]{\gdef\@subtitle{#1}}
\newcommand{\@periodo}{}
\newcommand{\periodo}[1]{\gdef\@periodo{#1}}
\newcommand{\@institucion}{Secretaría de Energía}
\newcommand{\institucion}[1]{\gdef\@institucion{#1}}
\newcommand{\@subsecretaria}{Subsecretaría de Planeación y Transición Energética}
\newcommand{\subsecretaria}[1]{\gdef\@subsecretaria{#1}}
\newcommand{\@unidad}{}
\newcommand{\unidad}[1]{\gdef\@unidad{#1}}
\newcommand{\@unidadsigla}{}
\newcommand{\unidadsigla}[1]{\gdef\@unidadsigla{#1}}
\newcommand{\@version}{1.0}
\newcommand{\version}[1]{\gdef\@version{#1}}
\newcommand{\@fecha}{\today}
\newcommand{\fecha}[1]{\gdef\@fecha{#1}}
\newcommand{\@fuentes}{SENER}
\newcommand{\fuentes}[1]{\gdef\@fuentes{#1}}
\newcommand{\@palabrasclave}{}
\newcommand{\palabrasclave}[1]{\gdef\@palabrasclave{#1}}
\newcommand{\@portadaimagen}{assets/reno_portada.png}
\newcommand{\portadaimagen}[1]{\gdef\@portadaimagen{#1}}
\newcommand{\@seccionimagen}{assets/reno_anexos.png}
\newcommand{\seccionimagen}[1]{\gdef\@seccionimagen{#1}}
\newcommand{\@contraportadaimagen}{assets/reno_anexos.png}
\newcommand{\contraportadaimagen}[1]{\gdef\@contraportadaimagen{#1}}
\newcommand{\DocumentoCorto}{Informe institucional}
\newcommand{\setDocumentoCorto}[1]{\renewcommand{\DocumentoCorto}{#1}}

\AtBeginDocument{
  \hypersetup{
    colorlinks=true,
    linkcolor=SENERGuinda,
    urlcolor=SENERGuinda,
    citecolor=SENEREsmeralda,
    pdfborder={0 0 0},
    pdftitle={\@title},
    pdfauthor={\@institucion\space--\space\@subsecretaria},
    pdfsubject={\@subtitle},
    pdfkeywords={\@palabrasclave},
    pdfcreator={XeLaTeX con clase institucional SENER},
    pdflang={es-MX}
  }
}
\makeatother

% --------------------------------------------------------------------------
% Ritmo editorial y pagina
% --------------------------------------------------------------------------
\color{SENERTinta}
\setlength{\parindent}{0pt}
\setlength{\parskip}{5.5pt plus 1pt minus .5pt}
\linespread{1.08}
\raggedbottom
\widowpenalty=10000
\clubpenalty=10000
\displaywidowpenalty=10000
\setlength{\emergencystretch}{1.5em}

\newcommand{\SENERHeaderLogo}{%
  \IfFileExists{assets/logo_sener.png}{\includegraphics[height=.55cm]{assets/logo_sener.png}}{\senerdisplay\bfseries\color{SENERGuinda}Energía}%
}

% Boton vectorial de retorno al indice. El destino se inserta de manera
% automatica al inicio de \tableofcontents. La casa funciona como acceso
% directo al indice y conserva una escala discreta junto al numero de pagina.
\newcommand{\SENERBackToIndex}{%
  \hyperlink{sener-indice}{%
    \XeTeXLinkBox{%
      \begin{tikzpicture}[baseline=-.55ex,line cap=round,line join=round]
        \draw[SENERGuinda,line width=.65pt]
          (-.145,.075) -- (0,.205) -- (.145,.075);
        \draw[SENERGuinda,line width=.65pt]
          (-.115,.075) -- (-.115,-.105) -- (.115,-.105) -- (.115,.075);
        \draw[SENERGuinda,line width=.55pt]
          (-.035,-.105) -- (-.035,-.025) -- (.035,-.025) -- (.035,-.105);
      \end{tikzpicture}%
    }%
  }%
}

\newcommand{\SENERIndice}{%
  \phantomsection
  \hypertarget{sener-indice}{}%
  \tableofcontents
}

\fancypagestyle{sener}{
  \fancyhf{}
  \fancyhead[L]{\SENERHeaderLogo}
  \fancyhead[R]{\senermicro\color{SENERGris}\MakeUppercase{\DocumentoCorto}}
  \fancyfoot[L]{\senermicro\color{SENERGrisClaro}SECRETARÍA DE ENERGÍA}
  \fancyfoot[C]{\textcolor{SENERDorado}{\raisebox{1.2pt}{\rule{1.7cm}{.6pt}}\,\raisebox{-.2pt}{\rotatebox{45}{\rule{4pt}{4pt}}}\,\raisebox{1.2pt}{\rule{1.7cm}{.6pt}}}}
  \fancyfoot[R]{\SENERBackToIndex\hspace{.65em}\senermono\bfseries\color{SENERTinta}\thepage}
  \renewcommand{\headrulewidth}{.5pt}
  \renewcommand{\headrule}{\hbox to\headwidth{\color{SENERLinea}\leaders\hrule height \headrulewidth\hfill}}
  \renewcommand{\footrulewidth}{0pt}
}
\fancypagestyle{plain}{
  \fancyhf{}
  \fancyhead[L]{\SENERHeaderLogo}
  \fancyhead[R]{\senermicro\color{SENERGris}\MakeUppercase{\DocumentoCorto}}
  \fancyfoot[L]{\senermicro\color{SENERGrisClaro}SECRETARÍA DE ENERGÍA}
  \fancyfoot[C]{\textcolor{SENERDorado}{\raisebox{1.2pt}{\rule{1.7cm}{.6pt}}\,\raisebox{-.2pt}{\rotatebox{45}{\rule{4pt}{4pt}}}\,\raisebox{1.2pt}{\rule{1.7cm}{.6pt}}}}
  \fancyfoot[R]{\SENERBackToIndex\hspace{.65em}\senermono\bfseries\color{SENERTinta}\thepage}
  \renewcommand{\headrulewidth}{.5pt}
  \renewcommand{\headrule}{\hbox to\headwidth{\color{SENERLinea}\leaders\hrule height \headrulewidth\hfill}}
  \renewcommand{\footrulewidth}{0pt}
}
\pagestyle{sener}

% --------------------------------------------------------------------------
% Jerarquia tipografica
% --------------------------------------------------------------------------
\setcounter{secnumdepth}{3}
\setcounter{tocdepth}{2}

\titleformat{\section}[display]
  {\senerdisplay\bfseries\fontsize{22pt}{24pt}\selectfont\color{SENERTinta}}
  {\senerkickerfont\color{SENERGuinda}\MakeUppercase{SECCIÓN \thesection}}
  {4pt}{}
\titlespacing*{\section}{0pt}{2.0em}{.8em}

\titleformat{\subsection}
  {\senerdisplay\bfseries\fontsize{15.5pt}{18pt}\selectfont\color{SENERGuinda}}
  {\thesubsection}{.55em}{}
\titlespacing*{\subsection}{0pt}{1.5em}{.55em}

\titleformat{\subsubsection}
  {\sffamily\bfseries\fontsize{11.2pt}{13.5pt}\selectfont\color{SENERTinta}}
  {\thesubsubsection}{.55em}{}
\titlespacing*{\subsubsection}{0pt}{1.2em}{.4em}

\titleformat{\paragraph}[runin]
  {\sffamily\bfseries\color{SENERDoradoSuave}}
  {\theparagraph}{.5em}{}[.]

% --------------------------------------------------------------------------
% Indice
% --------------------------------------------------------------------------
\renewcommand{\contentsname}{Índice}
\renewcommand{\cfttoctitlefont}{\senerdisplay\bfseries\fontsize{24pt}{28pt}\selectfont\color{SENERTinta}}
\renewcommand{\cftaftertoctitle}{\par\vspace{5pt}\color{SENERDorado}\hrule height 1.2pt\vspace{12pt}}
\renewcommand{\cftsecfont}{\sffamily\bfseries\color{SENERTinta}}
\renewcommand{\cftsecpagefont}{\senermono\bfseries\color{SENERGuinda}}
\renewcommand{\cftsubsecfont}{\sffamily\color{SENERGris}}
\renewcommand{\cftsubsecpagefont}{\senermono\color{SENERGris}}
\setlength{\cftbeforesecskip}{5pt}
\setlength{\cftbeforesubsecskip}{1.5pt}

% --------------------------------------------------------------------------
% Listas, citas y destacados
% --------------------------------------------------------------------------
\newcommand{\senerbullet}{\tikz[baseline=-.55ex]\fill[SENERGuinda] (0,0) circle (2.15pt);}
\newcommand{\senerring}{\tikz[baseline=-.55ex]\draw[SENERDorado,line width=.8pt] (0,0) circle (2.15pt);}
\setlist[itemize,1]{label=\senerbullet,leftmargin=1.45em,itemsep=3pt,topsep=4pt}
\setlist[itemize,2]{label=\senerring,leftmargin=1.6em,itemsep=2pt,topsep=2pt}
\setlist[itemize,3]{label=\textcolor{SENERGrisClaro}{--},leftmargin=1.7em,itemsep=1pt,topsep=1pt}
\setlist[enumerate,1]{label=\senerdisplay\bfseries\color{SENERGuinda}\arabic*.,leftmargin=1.8em,itemsep=3pt,topsep=4pt}

\newtcolorbox{senercallout}[1][Nota]{
  enhanced,
  breakable,
  colback=SENERPapel,
  colframe=SENERPapel,
  boxrule=0pt,
  borderline west={3pt}{0pt}{SENERDorado},
  arc=1mm,
  left=10pt,right=10pt,top=8pt,bottom=8pt,
  before skip=8pt,after skip=8pt,
  title=#1,
  coltitle=SENERGuinda,
  fonttitle=\sffamily\bfseries\small
}

\renewenvironment{quote}{\begin{senercallout}[Hallazgo]\itshape}{\end{senercallout}}

\newcommand{\SENERKicker}[1]{{\senerkickerfont\color{SENERGuinda}\MakeUppercase{#1}}}
\newcommand{\SENERSeparador}[1][SENERDorado]{%
  \par\vspace{5pt}\noindent
  \begin{tikzpicture}[x=1pt,y=1pt,baseline]
    \draw[#1,line width=.7pt] (0,0)--(.47\linewidth,0);
    \fill[#1,rotate=45] (.5\linewidth,-3.2pt) rectangle +(6.4pt,6.4pt);
    \draw[#1,line width=.7pt] (.53\linewidth,0)--(\linewidth,0);
  \end{tikzpicture}\par\vspace{7pt}
}

% --------------------------------------------------------------------------
% Diagramas vectoriales
% --------------------------------------------------------------------------
\tikzset{
  sener flow/.style={font=\sffamily\fontsize{7.3pt}{8.8pt}\selectfont,>=Latex},
  sener node/.style={
    draw=SENERLinea,
    line width=.7pt,
    fill=white,
    rounded corners=2pt,
    align=center,
    inner xsep=7pt,
    inner ysep=6pt,
    text=SENERTinta
  },
  sener guinda/.style={sener node,draw=SENERGuinda,fill=SENERGuinda!7},
  sener dorado/.style={sener node,draw=SENERDorado,fill=SENERDorado!10},
  sener verde/.style={sener node,draw=SENEREsmeralda,fill=SENEREsmeralda!7},
  sener cian/.style={sener node,draw=SENERCian,fill=SENERCian!7},
  sener gris/.style={sener node,draw=SENERGrisClaro,fill=SENERPapel},
  sener arrow/.style={-Latex,line width=.9pt,draw=SENERGuinda},
  sener arrow secondary/.style={-Latex,line width=.75pt,draw=SENERDorado},
  sener arrow data/.style={-Latex,line width=.75pt,draw=SENERCian,dashed},
  sener phase/.style={
    font=\senerkickerfont,
    text=SENERGris,
    align=center
  }
}

\NewDocumentEnvironment{SENERDiagram}{O{}}{%
  \par\vspace{8pt}
  \begin{center}
  \begin{minipage}{\textwidth}
  \centering
}{%
  \ifstrempty{#1}{}{\captionof{figure}{#1}}
  \end{minipage}
  \end{center}
  \vspace{7pt}
}

% --------------------------------------------------------------------------
% Tablas, cifras, pies y enlaces
% --------------------------------------------------------------------------
\arrayrulecolor{SENERLinea}
\renewcommand{\arraystretch}{1.18}
\setlength{\LTpre}{6pt}
\setlength{\LTpost}{8pt}
\setlength{\aboverulesep}{2pt}
\setlength{\belowrulesep}{3pt}
\AtBeginEnvironment{longtable}{\small\sffamily\setlength{\tabcolsep}{4pt}\rowcolors{2}{white}{SENERPapel}}
\newcommand{\senerth}[1]{{\sffamily\bfseries\fontsize{7.8pt}{9.4pt}\selectfont\color{SENERGuinda}\MakeUppercase{#1}}}
\newcommand{\SENERNum}[1]{{\senermono\bfseries #1}}
\newcommand{\SENERFuente}[1]{\par\vspace{2pt}{\senercaption\color{SENERGrisClaro}Fuente: #1}\par}
\newcolumntype{Y}{>{\RaggedRight\arraybackslash}X}
\newcolumntype{N}{>{\RaggedLeft\arraybackslash\senermono}X}

\captionsetup{
  font={small,color=SENERGris},
  labelfont={bf,color=SENERGuinda},
  labelsep=period,
  justification=raggedright,
  singlelinecheck=false
}

% --------------------------------------------------------------------------
% Portada y ficha documental
% --------------------------------------------------------------------------
\makeatletter
\newcommand{\SENERMetaLabel}[1]{{\sffamily\bfseries\fontsize{7.2pt}{8.6pt}\selectfont\addfontfeatures{LetterSpace=10}\color{SENERGrisClaro}\MakeUppercase{#1}}}

\renewcommand{\maketitle}{%
  \begin{titlepage}
    \thispagestyle{empty}
    \begin{tikzpicture}[remember picture,overlay]
      \fill[white] (current page.south west) rectangle (current page.north east);
      \fill[SENERGuinda]
        (current page.north west) rectangle ([yshift=-5.5pt]current page.north east);

      \node[anchor=north west,inner sep=0] at
        ([xshift=1.55cm,yshift=-1.18cm]current page.north west) {%
          \IfFileExists{assets/logo_gob.png}{\includegraphics[height=1.08cm]{assets/logo_gob.png}}{\senerdisplay Gobierno de México}%
          \hspace{.72em}\textcolor{SENERLinea}{\rule{.5pt}{.98cm}}\hspace{.72em}%
          \IfFileExists{assets/logo_sener.png}{\includegraphics[height=.82cm]{assets/logo_sener.png}}{\senerdisplay Energía}%
        };

      \node[anchor=north west,text width=18.35cm,align=left,inner sep=0] at
        ([xshift=1.55cm,yshift=-3.35cm]current page.north west) {%
          {\sffamily\bfseries\fontsize{8.2pt}{10.2pt}\selectfont
            \addfontfeatures{LetterSpace=8}\color{SENERGuinda}\MakeUppercase{\@subsecretaria}}\\[5pt]
          {\sffamily\fontsize{7.7pt}{10pt}\selectfont\color{SENERGris}%
            \ifstrempty{\@unidadsigla}{}{\textbf{\color{SENERGuinda}\@unidadsigla}\space\textcolor{SENERDorado}{·}\space}%
            \@unidad}
        };

      \node[anchor=north west,text width=18.35cm,align=left,inner sep=0] at
        ([xshift=1.55cm,yshift=-5.20cm]current page.north west) {%
          \senerdisplay\bfseries\fontsize{29.5pt}{31.3pt}\selectfont\color{SENERTinta}\@title
        };

      \node[anchor=north west,inner sep=0] at
        ([xshift=1.55cm,yshift=-10.05cm]current page.north west) {%
          \senerdisplay\bfseries\fontsize{18pt}{21pt}\selectfont\color{SENERGuinda}\@periodo
        };
      \draw[SENERDorado,line width=1.55pt]
        ([xshift=4.70cm,yshift=-10.48cm]current page.north west) --
        ([xshift=5.95cm,yshift=-10.48cm]current page.north west);
      \node[anchor=north west,text width=13.45cm,align=left,inner sep=0] at
        ([xshift=6.45cm,yshift=-9.86cm]current page.north west) {%
          \sffamily\fontsize{9.5pt}{12.5pt}\selectfont\color{SENERGris}\@subtitle
        };

      \IfFileExists{\@portadaimagen}{%
        \begin{scope}
          \clip ([yshift=-13.25cm]current page.north west) rectangle
                ([yshift=-20.75cm]current page.north east);
          \node[anchor=center,inner sep=0] at
            ([yshift=-17.0cm]current page.north) {%
              \includegraphics[width=\paperwidth]{\@portadaimagen}%
            };
        \end{scope}
        \fill[SENERDorado]
          ([yshift=-13.25cm]current page.north west) rectangle
          ([yshift=-13.19cm]current page.north east);
      }{}

      \node[anchor=north west,text width=4.85cm,align=left,inner sep=0] at
        ([xshift=1.55cm,yshift=-21.55cm]current page.north west) {%
          \SENERMetaLabel{Subsecretaría}\\[6pt]
          {\sffamily\bfseries\fontsize{8.4pt}{11.3pt}\selectfont\color{SENERTinta}\@subsecretaria}
        };

      \node[anchor=north west,text width=7.45cm,align=left,inner sep=0] at
        ([xshift=7.05cm,yshift=-21.55cm]current page.north west) {%
          \SENERMetaLabel{Unidad responsable}\\[6pt]
          {\sffamily\fontsize{8.25pt}{11pt}\selectfont\color{SENERTinta}%
            \ifstrempty{\@unidadsigla}{}{\textbf{\color{SENERGuinda}\@unidadsigla}\\[1pt]}%
            \@unidad}
        };

      \node[anchor=north west,text width=4.6cm,align=left,inner sep=0] at
        ([xshift=15.25cm,yshift=-21.55cm]current page.north west) {%
          \SENERMetaLabel{Control documental}\\[6pt]
          {\sffamily\fontsize{8.25pt}{11pt}\selectfont\color{SENERTinta}\raggedright
            \hyphenpenalty=10000\exhyphenpenalty=10000%
            \textbf{Corte:} \@fecha\\
            \textbf{Versión:} \@version\\
            \textbf{Fuentes:}\\[-1pt]
            {\fontsize{7.75pt}{10pt}\selectfont\@fuentes}}
        };

      \draw[SENERLinea,line width=.45pt]
        ([xshift=6.72cm,yshift=-21.55cm]current page.north west) --
        ([xshift=6.72cm,yshift=-25.95cm]current page.north west);
      \draw[SENERLinea,line width=.45pt]
        ([xshift=14.90cm,yshift=-21.55cm]current page.north west) --
        ([xshift=14.90cm,yshift=-25.95cm]current page.north west);
      \fill[SENERGuinda]
        (current page.south west) rectangle ([yshift=.13cm]current page.south east);
    \end{tikzpicture}
    \null
  \end{titlepage}
  \setcounter{page}{1}
}

\newcommand{\SENERSectionNumber}{\ifnum\value{section}<10 0\fi\arabic{section}}

% Portadilla completa para cada seccion principal. El segundo argumento es una
% descripcion breve que orienta al lector antes del contenido.
\NewDocumentCommand{\SENERSection}{O{40pt} O{43.5pt} m m}{%
  \clearpage
  \refstepcounter{section}%
  \phantomsection
  \addcontentsline{toc}{section}{\protect\numberline{\thesection}#3}%
  \markboth{#3}{#3}%
  \begingroup
    \thispagestyle{empty}
    \begin{tikzpicture}[remember picture,overlay]
      \fill[SENERPapel] (current page.south west) rectangle (current page.north east);

      \node[anchor=north west,inner sep=0] at
        ([xshift=1.65cm,yshift=-1.50cm]current page.north west) {%
          \IfFileExists{assets/logo_gob.png}{\includegraphics[height=.98cm]{assets/logo_gob.png}}{\senerdisplay Gobierno de México}%
          \hspace{.72em}\textcolor{SENERLinea}{\rule{.5pt}{.91cm}}\hspace{.72em}%
          \IfFileExists{assets/logo_sener.png}{\includegraphics[height=.76cm]{assets/logo_sener.png}}{\senerdisplay Energía}%
        };

      \node[anchor=north east,inner sep=0,text=SENERLinea!68] at
        ([xshift=-1.45cm,yshift=-10.15cm]current page.north east) {%
          \senerdisplay\bfseries\fontsize{140pt}{140pt}\selectfont\SENERSectionNumber
        };

      \node[anchor=north west,text width=12.25cm,align=left,inner sep=0]
        (senersectiontitle\arabic{section}) at
        ([xshift=1.65cm,yshift=-10.18cm]current page.north west) {%
          {\sffamily\bfseries\fontsize{9.2pt}{11.2pt}\selectfont\addfontfeatures{LetterSpace=12}%
           \color{SENERGuinda}\MakeUppercase{SECCIÓN \SENERSectionNumber}}\\[10.5pt]
          {\senerdisplay\bfseries\fontsize{#1}{#2}\selectfont\color{SENERTinta}%
           \raggedright\hyphenchar\font=-1 #3}
        };

      \draw[SENERDorado,line width=.75pt]
        ([yshift=-.52cm]senersectiontitle\arabic{section}.south west) --
        ([xshift=12.75cm,yshift=-.52cm]senersectiontitle\arabic{section}.south west);

      \node[anchor=north west,text width=13.35cm,align=left,inner sep=0] at
        ([yshift=-.98cm]senersectiontitle\arabic{section}.south west) {%
          \sffamily\fontsize{11pt}{15pt}\selectfont\color{SENERGris}%
          \raggedright\hyphenchar\font=-1\hyphenpenalty=10000\exhyphenpenalty=10000 #4
        };

      \IfFileExists{\@seccionimagen}{%
        \begin{scope}
          \clip ([yshift=.20cm]current page.south west) rectangle
                ([yshift=9.80cm]current page.south east);
          \node[anchor=center,inner sep=0] at
            ([yshift=4.95cm]current page.south) {%
            \includegraphics[width=\paperwidth]{\@seccionimagen}%
          };
        \end{scope}
      }{}

      % La franja se dibuja sobre la fotografia para conservar un borde recto,
      % continuo y sin halo lateral.
      \fill[SENERGuinda]
        (current page.north west) rectangle ([xshift=8.5pt]current page.south west);

      \fill[SENERTinta]
        (current page.south west) rectangle ([yshift=.17cm]current page.south east);
      \fill[SENERDorado]
        ([yshift=.17cm]current page.south west) rectangle
        ([yshift=.22cm]current page.south east);

      \node[anchor=south west,inner sep=0] at
        ([xshift=1.65cm,yshift=.78cm]current page.south west) {%
          \senermicro\color{white}\MakeUppercase{\DocumentoCorto}%
        };
    \end{tikzpicture}
    \null
    \clearpage
  \endgroup
  \pagestyle{sener}
}

% Cierre visual del informe.
\NewDocumentCommand{\SENERContraportada}{O{Documento de consulta institucional}}{%
  \clearpage
  \begingroup
    \thispagestyle{empty}
    \begin{tikzpicture}[remember picture,overlay]
      \fill[white] (current page.south west) rectangle (current page.north east);
      \fill[SENERGuinda]
        (current page.north west) rectangle ([xshift=5.5pt]current page.south west);

      \node[anchor=north west,inner sep=0] at
        ([xshift=1.9cm,yshift=-1.45cm]current page.north west) {%
          \IfFileExists{assets/logo_gob.png}{\includegraphics[height=1.0cm]{assets/logo_gob.png}}{\senerdisplay Gobierno de México}%
          \hspace{.7em}\textcolor{SENERLinea}{\rule{.5pt}{.92cm}}\hspace{.7em}%
          \IfFileExists{assets/logo_sener.png}{\includegraphics[height=.76cm]{assets/logo_sener.png}}{\senerdisplay Energía}%
        };

      \node[anchor=west,text width=.76\paperwidth,align=left,inner sep=0] at
        ([xshift=1.9cm,yshift=2.2cm]current page.west) {%
          {\senerkickerfont\color{SENERGuinda}\MakeUppercase{#1}}\\[10pt]
          {\senerdisplay\bfseries\fontsize{24pt}{27pt}\selectfont\color{SENERTinta}\@title}\\[7pt]
          {\senerdisplay\bfseries\fontsize{15pt}{18pt}\selectfont\color{SENERGuinda}\@periodo}\\[16pt]
          \textcolor{SENERDorado}{\rule{1.4cm}{2.2pt}}\\[14pt]
          {\sffamily\fontsize{9.5pt}{13pt}\selectfont\color{SENERGris}%
            \@institucion\\%
            \@subsecretaria\\%
            \ifstrempty{\@unidadsigla}{}{\textbf{\@unidadsigla}\space·\space}\@unidad\\%
            Fecha de corte: \@fecha}
        };

      \IfFileExists{\@contraportadaimagen}{%
        \begin{scope}
          \clip (current page.south west) rectangle ([yshift=7.45cm]current page.south east);
          \node[anchor=south,inner sep=0] at (current page.south) {%
            \includegraphics[width=\paperwidth]{\@contraportadaimagen}%
          };
        \end{scope}
        \fill[SENERDorado]
          ([yshift=7.45cm]current page.south west) rectangle
          ([yshift=7.51cm]current page.south east);
      }{}

      \node[anchor=south east,inner sep=0] at
        ([xshift=-1.2cm,yshift=.7cm]current page.south east) {%
          \senermicro\bfseries\color{white}GOBIERNO DE MÉXICO · ENERGÍA
        };
    \end{tikzpicture}
    \null
    \clearpage
  \endgroup
}

\NewDocumentCommand{\SENERFichaDocumental}{m}{%
  \section*{Ficha documental y alcance}
  \addcontentsline{toc}{section}{Ficha documental y alcance}
  \begin{tabularx}{\textwidth}{@{}>{\sffamily\bfseries\color{SENERGuinda}}p{.24\textwidth}Y@{}}
    Institución & \@institucion \\
    Subsecretaría & \@subsecretaria \\
    Unidad & \ifstrempty{\@unidadsigla}{}{\textbf{\@unidadsigla}\space·\space}\@unidad \\
    Versión & \@version \\
    Fecha de corte & \@fecha \\
    Fuentes prioritarias & \@fuentes \\
  \end{tabularx}
  \vspace{6pt}
  \begin{senercallout}[Nota de alcance]
    #1
  \end{senercallout}
  \clearpage
}
\makeatother

\endinput
