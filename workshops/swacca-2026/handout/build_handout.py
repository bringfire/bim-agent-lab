"""Build the eight-page Letter participant packet from content.json.

Dependencies: reportlab and pypdf. No Revit or network access.
The source uses only ASCII punctuation for reliable print rendering.
"""
from pathlib import Path
import argparse
import json
from xml.sax.saxutils import escape

from reportlab.lib import colors
from reportlab.lib.enums import TA_LEFT
from reportlab.lib.pagesizes import letter
from reportlab.lib.styles import ParagraphStyle
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak,
    Flowable, KeepTogether,
)
from reportlab.graphics.barcode.qr import QrCodeWidget
from reportlab.graphics.shapes import Drawing
from pypdf import PdfReader

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
WIDTH = 510
INK = colors.HexColor('#151515')
GRAY = colors.HexColor('#555555')
PALE = colors.HexColor('#f3f3f3')


class NoteLines(Flowable):
    def __init__(self, count):
        Flowable.__init__(self)
        self.width, self.height, self.count = WIDTH, count * 21, count

    def draw(self):
        self.canv.setStrokeColor(colors.HexColor('#aaaaaa'))
        self.canv.setLineWidth(0.45)
        for n in range(self.count):
            y = self.height - (n + 1) * 21 + 5
            self.canv.line(0, y, self.width, y)


def build(output):
    data = json.loads((HERE / 'content.json').read_text(encoding='utf-8'))
    # DejaVu is the fallback for rebuilding on Linux; fonts remain local resources.
    candidates = [
        (Path('C:/Windows/Fonts'), ['segoeui.ttf', 'segoeuib.ttf', 'consola.ttf']),
        (Path('/usr/share/fonts/truetype/dejavu'), ['DejaVuSans.ttf', 'DejaVuSans-Bold.ttf', 'DejaVuSansMono.ttf']),
    ]
    for directory, files in candidates:
        if all((directory / name).is_file() for name in files):
            for alias, name in zip(['Body', 'Strong', 'Mono'], files):
                pdfmetrics.registerFont(TTFont(alias, str(directory / name)))
            break
    else:
        raise RuntimeError('Install Segoe UI or DejaVu Sans/Mono before rebuilding.')
    pdfmetrics.registerFontFamily('Body', normal='Body', bold='Strong', italic='Body', boldItalic='Strong')
    styles = {
        'p': ParagraphStyle('p', fontName='Body', fontSize=10.5, leading=14.3, textColor=INK, spaceAfter=9),
        'small': ParagraphStyle('small', fontName='Body', fontSize=8.5, leading=11.5, textColor=GRAY, spaceAfter=6),
        'h2': ParagraphStyle('h2', fontName='Strong', fontSize=12.3, leading=16, spaceBefore=4, spaceAfter=6, keepWithNext=True),
        'title': ParagraphStyle('title', fontName='Strong', fontSize=25, leading=29, spaceAfter=7),
        'subtitle': ParagraphStyle('subtitle', fontName='Body', fontSize=11, leading=15, textColor=GRAY, spaceAfter=16),
        'cell': ParagraphStyle('cell', fontName='Body', fontSize=9.4, leading=12.5),
        'head': ParagraphStyle('head', fontName='Strong', fontSize=9.3, leading=12),
        'code': ParagraphStyle('code', fontName='Mono', fontSize=10, leading=15, spaceAfter=9),
    }
    def para(text, kind='p'):
        return Paragraph(escape(text).replace('\n', '<br/>'), styles[kind])

    def box(title, text, shade=False):
        t = Table([[para(title, 'head')], [para(text)]], colWidths=[WIDTH])
        t.setStyle(TableStyle([
            ('BOX', (0, 0), (-1, -1), .65, INK),
            ('BACKGROUND', (0, 0), (-1, -1), PALE if shade else colors.white),
            ('LEFTPADDING', (0, 0), (-1, -1), 11), ('RIGHTPADDING', (0, 0), (-1, -1), 11),
            ('TOPPADDING', (0, 0), (-1, 0), 9), ('BOTTOMPADDING', (0, -1), (-1, -1), 8),
        ]))
        return [t, Spacer(1, 11)]

    story = []
    for index, page in enumerate(data['pages']):
        if index:
            story.append(PageBreak())
        story.extend([para(page['title'], 'title'), para(page['subtitle'], 'subtitle')])
        for block in page['blocks']:
            kind = block['type']
            if kind in ('p', 'h2', 'small', 'code'):
                story.append(para(block['text'], kind))
            elif kind in ('prompt', 'callout'):
                story.extend(box(block['title'], block['text'], shade=kind == 'callout'))
            elif kind == 'steps':
                for i, item in enumerate(block['items'], 1):
                    story.append(para(f'{i}. {item}'))
            elif kind == 'table':
                rows = [[para(x, 'head') for x in block['headers']]]
                rows += [[para(x, 'cell') for x in row] for row in block['rows']]
                table = Table(rows, colWidths=[WIDTH * x for x in block['widths']], hAlign='LEFT')
                table.setStyle(TableStyle([
                    ('VALIGN', (0, 0), (-1, -1), 'TOP'),
                    ('BACKGROUND', (0, 0), (-1, 0), PALE),
                    ('LINEBELOW', (0, 0), (-1, 0), .8, INK),
                    ('LINEBELOW', (0, 1), (-1, -1), .4, colors.HexColor('#bbbbbb')),
                    ('TOPPADDING', (0, 0), (-1, -1), 6), ('BOTTOMPADDING', (0, 0), (-1, -1), 6),
                    ('LEFTPADDING', (0, 0), (-1, -1), 7), ('RIGHTPADDING', (0, 0), (-1, -1), 7),
                ]))
                story.extend([table, Spacer(1, 11)])
            elif kind == 'lines':
                story.extend([para(block['title'], 'head'), NoteLines(block['count']), Spacer(1, 8)])
            elif kind == 'flow':
                cells = []
                for n, item in enumerate(block['items']):
                    if n:
                        cells.append(para('>', 'head'))
                    cells.append(para(item, 'head'))
                table = Table([cells], colWidths=[152, 27, 152, 27, 152])
                table.setStyle(TableStyle([('BOX', (x, 0), (x, 0), .6, INK) for x in (0, 2, 4)] + [
                    ('TOPPADDING', (0, 0), (-1, -1), 10), ('BOTTOMPADDING', (0, 0), (-1, -1), 10),
                ]))
                story.extend([table, Spacer(1, 10)])
            elif kind == 'repo':
                qr = QrCodeWidget(data['repo_url'], barLevel='M')
                bounds = qr.getBounds()
                scale = 76 / (bounds[2] - bounds[0])
                drawing = Drawing(76, 76, transform=[scale, 0, 0, scale, 0, 0])
                drawing.add(qr)
                link = Paragraph('<link href="' + data['repo_url'] + '"><b>' + escape(block['text']) + '</b></link>', styles['p'])
                table = Table([[[link, para(block['caption'], 'small')], drawing]], colWidths=[408, 102])
                table.setStyle(TableStyle([('VALIGN', (0, 0), (-1, -1), 'MIDDLE'), ('LEFTPADDING', (0, 0), (-1, -1), 0)]))
                story.extend([table, Spacer(1, 9)])
            elif kind == 'references':
                for item in block['items']:
                    story.append(Paragraph('<link href="' + escape(item['url'], {'"': '&quot;'}) + '">' + escape(item['label']) + '</link>', styles['small']))
            else:
                raise ValueError(f'Unknown block: {kind}')

    def decorate(canvas, doc):
        canvas.saveState()
        canvas.setTitle(data['title'])
        canvas.setAuthor('BIM Agent Lab')
        canvas.setFillColor(GRAY)
        canvas.setFont('Strong', 8)
        canvas.drawString(45, 759, 'BIM AGENT LAB / SWACCA 2026')
        label = data['pages'][doc.page - 1]['label'] if doc.page <= 8 else 'OVERFLOW'
        canvas.drawRightString(567, 759, label)
        canvas.setStrokeColor(INK)
        canvas.setLineWidth(.7)
        canvas.line(45, 747, 567, 747)
        canvas.setStrokeColor(colors.HexColor('#aaaaaa'))
        canvas.line(45, 42, 567, 42)
        canvas.setFont('Body', 8)
        canvas.drawString(45, 27, data['edition'])
        canvas.drawRightString(567, 27, f'{doc.page} / 8')
        canvas.restoreState()

    output.parent.mkdir(parents=True, exist_ok=True)
    doc = SimpleDocTemplate(str(output), pagesize=letter, leftMargin=45, rightMargin=45,
                            topMargin=62, bottomMargin=56, allowSplitting=1)
    doc.build(story, onFirstPage=decorate, onLaterPages=decorate)
    pdf = PdfReader(output)
    if len(pdf.pages) != 8:
        raise RuntimeError(f'Expected 8 pages, got {len(pdf.pages)}. Edit content/layout before delivery.')
    for i, page in enumerate(pdf.pages):
        assert list(page.mediabox) == [0, 0, 612, 792]
        assert data['pages'][i]['title'] in page.extract_text()
    print(f'Built and checked 8 Letter pages: {output}')


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', type=Path, default=ROOT / 'output/pdf/swacca-2026-participant-handout.pdf')
    build(parser.parse_args().output)
