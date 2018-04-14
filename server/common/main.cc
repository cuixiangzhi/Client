#define _CRT_SECURE_NO_WARNINGS
#pragma comment(linker,"/subsystem:\"windows\" /entry:\"mainCRTStartup\"")
#include <stdlib.h>
#include <gl/glut.h>

void on_dispaly()
{
	glClearColor(0, 0, 0, 1);
	glClear(GL_COLOR_BUFFER_BIT);
	glutSwapBuffers();
}

void on_mosue(int button,int state,int x,int y)
{

}

void on_key_down(unsigned char key,int x,int y)
{

}

void on_key_up(unsigned char key, int x, int y)
{

}

void on_reshape()
{

}

void on_menu(int state)
{

}

int main(int argc, char** argv)
{
	glutInit(&argc, argv);
	glutInitDisplayMode(GLUT_RGBA | GLUT_DOUBLE | GLUT_DEPTH | GLUT_ALPHA);
	int w = glutGet(GLUT_SCREEN_WIDTH);
	int h = glutGet(GLUT_SCREEN_HEIGHT);
	glutInitWindowPosition(0, 0);
	glutInitWindowSize(w, h);
	glutCreateWindow("Unity");
	glViewport(100, 100, 500, 500);

	glutCreateMenu(on_menu);

	glutDisplayFunc(on_dispaly);
	glutKeyboardFunc(on_key_down);
	glutKeyboardUpFunc(on_key_up);
	glutMouseFunc(on_mosue);
	glutMainLoop();
}