<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:outline="http://code.google.com/p/wkhtmltopdf/outline"
	xmlns="http://www.w3.org/1999/xhtml">
<xsl:output doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN"
	doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"
	indent="yes" />
<xsl:template match="outline:outline">
<html>
<head>
	<title>
		Sadržaj:
	</title>
    <style>
		body {
			font-family: "Tele-GroteskNor";
			font-size: 18pt;
		}
		.toc{
			font-size: 30pt;
			margin: 50pt 0 0 0;
		}
		/* 	We set the height of the DIV and place the link and page number absolutely.
			The DIV is filled with dots, but these are hidden by the link and span which have a white background.
		*/
		.toc-wrapper {
			position: relative;
			height: 24pt;
			line-height: 24pt;
			padding: 0;
			margin: 0;
			white-space: nowrap;
			overflow: hidden;
		}
		a, span {
			position: absolute;
			top: 0;
			display: inline-block;
			line-height: 24pt;
			background-color: white;
		}
		a {
			left: 0;
			text-decoration: none;
			color: black;
			padding-right: 5pt;
		}
		span {
			right: 0;
			text-align: right;
			padding-left: 5pt;
		}
		ul {
			padding-left: 0;
			list-style: none;
		}
		ul ul {
			font-size: 80%;
			padding-left: 1em;
		}
	</style>
</head>
<body>
	<div class="toc">Sadržaj:</div>
	<ul>
		<xsl:apply-templates select="outline:item/outline:item"/>
	</ul>
</body>
</html>
</xsl:template>

<xsl:template match="outline:item">
	<li>
		<xsl:if test="@title!=''">
			<div class="toc-wrapper">. . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .
				<a>
					<xsl:if test="@link">
						<xsl:attribute name="href"><xsl:value-of select="@link"/></xsl:attribute>
					</xsl:if>
					<xsl:if test="@backLink">
						<xsl:attribute name="name"><xsl:value-of select="@backLink"/></xsl:attribute>
					</xsl:if>
					<xsl:value-of select="@title" />
				</a>
				<span><xsl:value-of select="@page" /></span>
			</div>
		</xsl:if>
		<ul>
		  <xsl:apply-templates select="outline:item"/>
		</ul>
	</li>
</xsl:template>
</xsl:stylesheet>
